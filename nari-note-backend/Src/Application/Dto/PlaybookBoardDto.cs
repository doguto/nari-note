using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookBoardDto
{
    public PlaybookBoardId Id { get; set; }
    public PlaybookNodeId? NodeId { get; set; }
    public required string Sfen { get; set; }
}
