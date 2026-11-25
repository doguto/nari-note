using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class SaveBookLinesResponse
{
    public List<SavedPositionDto> Positions { get; set; } = new();
}
