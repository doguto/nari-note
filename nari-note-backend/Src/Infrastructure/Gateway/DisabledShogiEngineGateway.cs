using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

// エンジンサービスの URL が設定されていない環境で使う。解析は行わない
public class DisabledShogiEngineGateway : IShogiEngineGateway
{
    public bool IsEnabled => false;

    public Task<List<EngineLine>> AnalyzeAsync(string sfen, string engine, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<EngineLine>());
    }
}
