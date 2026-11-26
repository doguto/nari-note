namespace NariNoteBackend.Application.Dto.Response;

public class AnalyzePlaybookPositionResponse
{
    public bool IsAvailable { get; set; }
    public List<EngineLineDto> Lines { get; set; } = new();
}
