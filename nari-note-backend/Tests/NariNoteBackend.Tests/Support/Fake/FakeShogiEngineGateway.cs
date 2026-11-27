using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeShogiEngineGateway : IShogiEngineGateway
{
    public bool IsEnabled { get; set; } = true;
    public List<EngineLine> Lines { get; } = new();
    public List<string> AnalyzedEngines { get; } = new();

    public Task<List<EngineLine>> AnalyzeAsync(string sfen, string engine, CancellationToken cancellationToken = default)
    {
        this.AnalyzedEngines.Add(engine);
        return Task.FromResult(this.Lines.ToList());
    }

    public void Reset()
    {
        this.IsEnabled = true;
        this.Lines.Clear();
        this.AnalyzedEngines.Clear();
    }
}
