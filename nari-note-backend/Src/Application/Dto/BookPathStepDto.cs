using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookPathStepDto
{
    public BookPositionId ParentPositionId { get; set; }
    public BookPositionId PositionId { get; set; }
    public required string Usi { get; set; }
    public required string Label { get; set; }
}
