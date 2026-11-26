namespace NariNoteBackend.Application.Dto.Response;

public class AnalyzePlaybookBoardResponse
{
    public bool IsAvailable { get; set; }
    public List<EngineLineDto> Lines { get; set; } = new();
}
