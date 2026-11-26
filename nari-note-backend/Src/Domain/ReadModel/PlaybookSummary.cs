using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.ReadModel;

public class PlaybookSummary
{
    public required PlaybookId Id { get; init; }
    public required string Name { get; init; }
    public required int PositionCount { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
