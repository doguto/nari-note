namespace NariNoteBackend.Domain.Gateway;

public class EngineLine
{
    public required string Usi { get; init; }
    // 手番側から見た評価値（cp）。詰みは ±32000 付近
    public required int Score { get; init; }
    public required List<string> Pv { get; init; }
}

// 局面を解析するエンジンサービス。API サーバーとは別のマシンで動かす
public interface IShogiEngineGateway
{
    bool IsEnabled { get; }
    Task<List<EngineLine>> AnalyzeAsync(string sfen, string engine, CancellationToken cancellationToken = default);
}
