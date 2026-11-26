using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class SavedPositionDto
{
    public required string Sfen { get; set; }
    public PlaybookPositionId PositionId { get; set; }
}
