using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class SavePlaybookLinesResponse
{
    public List<SavedPositionDto> Positions { get; set; } = new();
}
