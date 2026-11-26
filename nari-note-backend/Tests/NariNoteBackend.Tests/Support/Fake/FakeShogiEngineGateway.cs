using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeShogiEngineGateway : IShogiEngineGateway
{
    public bool IsEnabled { get; set; } = true;
    public List<EngineLine> Lines { get; } = new();
    // 候補探しで返す局面。null のときは others をすべて 1 単位差の候補として返す
    public List<EngineCandidate>? Candidates { get; set; }
    public EngineVerdict Verdict { get; set; } = new() { Kind = "same", Delta = 0, ConfirmedDepth = 6 };
    public List<(string A, string B)> Judged { get; } = new();
    // 判定で投げる例外。null のときは Verdict を返す
    public System.Exception? JudgeException { get; set; }

    public List<string> AnalyzedEngines { get; } = new();

    public Task<List<EngineLine>> AnalyzeAsync(string sfen, string engine, CancellationToken cancellationToken = default)
    {
        this.AnalyzedEngines.Add(engine);
        return Task.FromResult(this.Lines.ToList());
    }

    public Task<List<EngineCandidate>> FindCandidatesAsync(
        string sfen,
        List<string> others,
        CancellationToken cancellationToken = default
    )
    {
        var result = this.Candidates ?? others.Select(o => new EngineCandidate { Sfen = o, UnitCount = 1 }).ToList();
        return Task.FromResult(result.Where(c => others.Contains(c.Sfen)).ToList());
    }

    public Task<EngineVerdict> JudgeAsync(string sfenA, string sfenB, CancellationToken cancellationToken = default)
    {
        this.Judged.Add((sfenA, sfenB));
        if (this.JudgeException != null) return Task.FromException<EngineVerdict>(this.JudgeException);
        return Task.FromResult(this.Verdict);
    }

    public void Reset()
    {
        this.IsEnabled = true;
        this.Lines.Clear();
        this.AnalyzedEngines.Clear();
        this.Candidates = null;
        this.Verdict = new EngineVerdict { Kind = "same", Delta = 0, ConfirmedDepth = 6 };
        this.Judged.Clear();
        this.JudgeException = null;
    }
}
