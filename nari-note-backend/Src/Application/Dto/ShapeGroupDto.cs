using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class ShapeGroupDto
{
    public ShapeGroupId Id { get; set; }
    public required string Name { get; set; }
    public required string Memo { get; set; }
    public List<string> NameSuggestions { get; set; } = new();
    public List<ShapeMemberDto> Members { get; set; } = new();
}
