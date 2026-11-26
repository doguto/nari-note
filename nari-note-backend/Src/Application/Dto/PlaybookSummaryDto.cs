using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookSummaryDto
{
    public PlaybookId Id { get; set; }
    public required string Name { get; set; }
    public int PositionCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
