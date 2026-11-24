using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookGraphEdgeDto
{
    public ShapeGroupId FromGroupId { get; set; }
    public ShapeGroupId ToGroupId { get; set; }
    public required string Label { get; set; }
    public required string Condition { get; set; }
    public bool IsConditional { get; set; }
}
