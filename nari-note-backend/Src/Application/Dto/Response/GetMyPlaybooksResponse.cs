namespace NariNoteBackend.Application.Dto.Response;

public class GetMyPlaybooksResponse
{
    public List<PlaybookSummaryDto> Playbooks { get; set; } = new();
}
