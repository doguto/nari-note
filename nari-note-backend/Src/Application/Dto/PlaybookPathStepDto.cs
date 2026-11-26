using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookPathStepDto
{
    public PlaybookPositionId ParentPositionId { get; set; }
    public PlaybookPositionId PositionId { get; set; }
    public required string Usi { get; set; }
    public required string Label { get; set; }
}
