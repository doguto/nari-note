using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookGraphDto
{
    public ShapeGroupId RootGroupId { get; set; }
    public ShapeGroupId CurrentGroupId { get; set; }
    public List<PlaybookGraphNodeDto> Nodes { get; set; } = new();
    public List<PlaybookGraphEdgeDto> Edges { get; set; } = new();
}
