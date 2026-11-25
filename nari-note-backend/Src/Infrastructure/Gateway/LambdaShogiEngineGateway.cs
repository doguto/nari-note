using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class LambdaShogiEngineGateway : IShogiEngineGateway
{
    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    readonly IAmazonLambda lambda;
    readonly string functionName;

    public LambdaShogiEngineGateway(IAmazonLambda lambda, string functionName)
    {
        this.lambda = lambda;
        this.functionName = functionName;
    }

    public bool IsEnabled => true;

    public async Task<List<EngineLine>> AnalyzeAsync(string sfen, CancellationToken cancellationToken = default)
    {
        var result = await this.InvokeAsync<AnalyzeResult>(new { action = "analyze", sfen }, cancellationToken);
        return result.Lines;
    }

    public async Task<List<EngineCandidate>> FindCandidatesAsync(
        string sfen,
        List<string> others,
        CancellationToken cancellationToken = default
    )
    {
        var result = await this.InvokeAsync<CandidatesResult>(new { action = "candidates", sfen, others }, cancellationToken);
        return result.Candidates;
    }

    public async Task<EngineVerdict> JudgeAsync(string sfenA, string sfenB, CancellationToken cancellationToken = default)
    {
        return await this.InvokeAsync<EngineVerdict>(new { action = "judge", p = sfenA, q = sfenB }, cancellationToken);
    }

    // 呼び出し側は HTTP のエンジンサービスと同じく HttpRequestException で失敗を扱うので、それに揃える
    async Task<T> InvokeAsync<T>(object payload, CancellationToken cancellationToken)
    {
        InvokeResponse response;
        try
        {
            response = await this.lambda.InvokeAsync(
                new InvokeRequest
                {
                    FunctionName = this.functionName,
                    InvocationType = InvocationType.RequestResponse,
                    Payload = JsonSerializer.Serialize(payload, JsonOptions),
                },
                cancellationToken
            );
        }
        catch (AmazonServiceException ex)
        {
            throw new HttpRequestException($"エンジンの呼び出しに失敗しました: {ex.Message}", ex);
        }

        using var reader = new StreamReader(response.Payload);
        var body = await reader.ReadToEndAsync(cancellationToken);
        if (!string.IsNullOrEmpty(response.FunctionError))
        {
            throw new HttpRequestException($"エンジンがエラーを返しました: {response.FunctionError} {body}");
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new HttpRequestException("エンジンが空の応答を返しました");
    }

    class AnalyzeResult
    {
        public List<EngineLine> Lines { get; init; } = new();
    }

    class CandidatesResult
    {
        public List<EngineCandidate> Candidates { get; init; } = new();
    }
}
