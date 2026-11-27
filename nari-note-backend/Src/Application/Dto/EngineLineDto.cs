namespace NariNoteBackend.Application.Dto;

public class EngineLineDto
{
    public required string Usi { get; set; }
    public required string Label { get; set; }
    // 先手から見た評価値
    public int Score { get; set; }
    public List<string> Pv { get; set; } = new();
}
