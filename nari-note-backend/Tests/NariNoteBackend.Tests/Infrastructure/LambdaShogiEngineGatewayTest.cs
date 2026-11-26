using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using NariNoteBackend.Infrastructure.Gateway;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NariNoteBackend.Tests.Infrastructure;

public class LambdaShogiEngineGatewayTest
{
    const string FunctionName = "nari-note-shogi-engine";

    readonly IAmazonLambda lambda = Substitute.For<IAmazonLambda>();
    readonly LambdaShogiEngineGateway gateway;
    InvokeRequest? sent;

    public LambdaShogiEngineGatewayTest()
    {
        this.gateway = new LambdaShogiEngineGateway(this.lambda, FunctionName);
    }

    void Respond(string body, string? functionError = null)
    {
        this.lambda.InvokeAsync(Arg.Do<InvokeRequest>(r => this.sent = r), Arg.Any<CancellationToken>())
            .Returns(_ => new InvokeResponse
            {
                StatusCode = 200,
                FunctionError = functionError,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes(body)),
            });
    }

    JsonElement SentPayload() => JsonDocument.Parse(this.sent!.Payload).RootElement;

    [Fact]
    public async Task 解析は_action_と局面とエンジンを送り読み筋を受け取る()
    {
        Respond("""{"lines": [{"usi": "2g2f", "score": 73, "pv": ["2g2f", "8c8d"]}]}""");

        var lines = await this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-micro");

        Assert.Equal(FunctionName, this.sent!.FunctionName);
        Assert.Equal(InvocationType.RequestResponse, this.sent.InvocationType);
        Assert.Equal("analyze", SentPayload().GetProperty("action").GetString());
        Assert.Equal("sfen-a", SentPayload().GetProperty("sfen").GetString());
        Assert.Equal("tsuyouraou-micro", SentPayload().GetProperty("engine").GetString());
        var line = Assert.Single(lines);
        Assert.Equal("2g2f", line.Usi);
        Assert.Equal(73, line.Score);
        Assert.Equal(["2g2f", "8c8d"], line.Pv);
    }

    [Fact]
    public async Task 関数がエラーを返したら_HttpRequestException_にする()
    {
        Respond("""{"errorMessage": "engine crashed"}""", functionError: "Unhandled");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-mini"));
        Assert.Contains("engine crashed", ex.Message);
    }

    [Fact]
    public async Task 同時実行数の上限で断られたら_429_の_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TooManyRequestsException("Rate exceeded"));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-micro"));
        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
    }

    [Fact]
    public async Task 接続できなかったら_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AmazonClientException("Unable to connect"));

        await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-mini"));
    }

    [Fact]
    public async Task クライアント側でタイムアウトしたら_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

        await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-mini"));
    }

    [Fact]
    public async Task 呼び出し元が止めたときはキャンセルのまま伝える()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => this.gateway.AnalyzeAsync("sfen-a", "tsuyouraou-mini", cts.Token));
    }
}
