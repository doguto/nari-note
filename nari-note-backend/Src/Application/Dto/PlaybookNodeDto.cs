using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookNodeDto
{
    public PlaybookNodeId Id { get; set; }
    public required string Name { get; set; }
    public required string Memo { get; set; }
}
