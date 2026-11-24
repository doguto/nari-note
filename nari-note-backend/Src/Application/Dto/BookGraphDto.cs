using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookGraphDto
{
    public ShapeGroupId RootGroupId { get; set; }
    public ShapeGroupId CurrentGroupId { get; set; }
    public List<BookGraphNodeDto> Nodes { get; set; } = new();
    public List<BookGraphEdgeDto> Edges { get; set; } = new();
}
