using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

// エンジンサービスの URL が設定されていない環境で使う。解析と判定は行わない
public class DisabledShogiEngineGateway : IShogiEngineGateway
{
    public bool IsEnabled => false;

    public Task<List<EngineLine>> AnalyzeAsync(string sfen, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<EngineLine>());
    }

    public Task<List<EngineCandidate>> FindCandidatesAsync(
        string sfen,
        List<string> others,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(new List<EngineCandidate>());
    }

    public Task<EngineVerdict> JudgeAsync(string sfenA, string sfenB, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("エンジンサービスが設定されていません");
    }
}
