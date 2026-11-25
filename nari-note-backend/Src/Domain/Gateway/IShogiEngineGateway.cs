namespace NariNoteBackend.Domain.Gateway;

public class EngineLine
{
    public required string Usi { get; init; }
    // 手番側から見た評価値（cp）。詰みは ±32000 付近
    public required int Score { get; init; }
    public required List<string> Pv { get; init; }
}

public class EngineCandidate
{
    public required string Sfen { get; init; }
    public required int UnitCount { get; init; }
}

public class EngineVerdict
{
    public required string Kind { get; init; }
    public int? Delta { get; init; }
    public int ConfirmedDepth { get; init; }
    public string Reason { get; init; } = "";
}

// 局面の解析と、2 局面が本質的に同じ形かの判定を行うエンジンサービス。API サーバーとは別のマシンで動かす
public interface IShogiEngineGateway
{
    bool IsEnabled { get; }
    Task<List<EngineLine>> AnalyzeAsync(string sfen, CancellationToken cancellationToken = default);
    Task<List<EngineCandidate>> FindCandidatesAsync(string sfen, List<string> others, CancellationToken cancellationToken = default);
    Task<EngineVerdict> JudgeAsync(string sfenA, string sfenB, CancellationToken cancellationToken = default);
}
