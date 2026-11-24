using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookGraphNodeDto
{
    public ShapeGroupId GroupId { get; set; }
    public BookPositionId RepresentativePositionId { get; set; }
    public required string MoveLabel { get; set; }
    public required string Name { get; set; }
    public int PositionCount { get; set; }
    public bool IsLeaf { get; set; }
    public bool HasMemo { get; set; }
}
