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
    public async Task 解析は_action_と局面を送り読み筋を受け取る()
    {
        Respond("""{"lines": [{"usi": "2g2f", "score": 73, "pv": ["2g2f", "8c8d"]}]}""");

        var lines = await this.gateway.AnalyzeAsync("sfen-a");

        Assert.Equal(FunctionName, this.sent!.FunctionName);
        Assert.Equal(InvocationType.RequestResponse, this.sent.InvocationType);
        Assert.Equal("analyze", SentPayload().GetProperty("action").GetString());
        Assert.Equal("sfen-a", SentPayload().GetProperty("sfen").GetString());
        var line = Assert.Single(lines);
        Assert.Equal("2g2f", line.Usi);
        Assert.Equal(73, line.Score);
        Assert.Equal(["2g2f", "8c8d"], line.Pv);
    }

    [Fact]
    public async Task 候補の絞り込みは比べる局面を送り差分の単位数を受け取る()
    {
        Respond("""{"candidates": [{"sfen": "sfen-b", "unit_count": 2}]}""");

        var candidates = await this.gateway.FindCandidatesAsync("sfen-a", ["sfen-b", "sfen-c"]);

        Assert.Equal("candidates", SentPayload().GetProperty("action").GetString());
        Assert.Equal(2, SentPayload().GetProperty("others").GetArrayLength());
        var candidate = Assert.Single(candidates);
        Assert.Equal("sfen-b", candidate.Sfen);
        Assert.Equal(2, candidate.UnitCount);
    }

    [Fact]
    public async Task 判定は2局面を_p_と_q_で送り判定結果を受け取る()
    {
        Respond("""{"kind": "same_with_delta", "delta": 35, "confirmed_depth": 6, "reason": ""}""");

        var verdict = await this.gateway.JudgeAsync("sfen-a", "sfen-b");

        Assert.Equal("judge", SentPayload().GetProperty("action").GetString());
        Assert.Equal("sfen-a", SentPayload().GetProperty("p").GetString());
        Assert.Equal("sfen-b", SentPayload().GetProperty("q").GetString());
        Assert.Equal("same_with_delta", verdict.Kind);
        Assert.Equal(35, verdict.Delta);
        Assert.Equal(6, verdict.ConfirmedDepth);
    }

    [Fact]
    public async Task 関数がエラーを返したら_HttpRequestException_にする()
    {
        Respond("""{"errorMessage": "engine crashed"}""", functionError: "Unhandled");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.JudgeAsync("sfen-a", "sfen-b"));
        Assert.Contains("engine crashed", ex.Message);
    }

    [Fact]
    public async Task 呼び出し自体に失敗したら_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TooManyRequestsException("Rate exceeded"));

        await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.AnalyzeAsync("sfen-a"));
    }

    [Fact]
    public async Task 接続できなかったら_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AmazonClientException("Unable to connect"));

        await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.JudgeAsync("sfen-a", "sfen-b"));
    }

    [Fact]
    public async Task クライアント側でタイムアウトしたら_HttpRequestException_にする()
    {
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

        await Assert.ThrowsAsync<HttpRequestException>(() => this.gateway.JudgeAsync("sfen-a", "sfen-b"));
    }

    [Fact]
    public async Task 呼び出し元が止めたときはキャンセルのまま伝える()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        this.lambda.InvokeAsync(Arg.Any<InvokeRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => this.gateway.JudgeAsync("sfen-a", "sfen-b", cts.Token));
    }
}
