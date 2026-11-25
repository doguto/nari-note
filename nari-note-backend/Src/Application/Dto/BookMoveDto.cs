using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookMoveDto
{
    public BookMoveId Id { get; set; }
    public required string Usi { get; set; }
    public required string Label { get; set; }
    public required string Memo { get; set; }
    public required string Condition { get; set; }
    public List<string> ConditionSuggestions { get; set; } = new();
    public BookPositionId ChildPositionId { get; set; }
}
