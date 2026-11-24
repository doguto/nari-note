using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class BookSummaryDto
{
    public BookId Id { get; set; }
    public required string Name { get; set; }
    public int PositionCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
