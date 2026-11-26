using System.Net.Http.Json;
using System.Text.Json;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class HttpShogiEngineGateway : IShogiEngineGateway
{
    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    readonly HttpClient httpClient;

    public HttpShogiEngineGateway(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public bool IsEnabled => true;

    public async Task<List<EngineLine>> AnalyzeAsync(string sfen, string engine, CancellationToken cancellationToken = default)
    {
        var result = await this.PostAsync<AnalyzeResult>("analyze", new { sfen, engine }, cancellationToken);
        return result.Lines;
    }

    public async Task<List<EngineCandidate>> FindCandidatesAsync(
        string sfen,
        List<string> others,
        CancellationToken cancellationToken = default
    )
    {
        var result = await this.PostAsync<CandidatesResult>("candidates", new { sfen, others }, cancellationToken);
        return result.Candidates;
    }

    public async Task<EngineVerdict> JudgeAsync(string sfenA, string sfenB, CancellationToken cancellationToken = default)
    {
        return await this.PostAsync<EngineVerdict>("judge", new { p = sfenA, q = sfenB }, cancellationToken);
    }

    async Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await this.httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"エンジンサービスの {path} が空の応答を返しました");
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
