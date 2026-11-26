using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookNodeBoardDto
{
    public PlaybookNodeBoardId Id { get; set; }
    public PlaybookNodeId NodeId { get; set; }
    public required string Sfen { get; set; }
}
