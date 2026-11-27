using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookBoardTransitionDto
{
    public PlaybookBoardTransitionId Id { get; set; }
    public PlaybookBoardId FromBoardId { get; set; }
    public PlaybookBoardId? ToBoardId { get; set; }
    public List<string> Usis { get; set; } = new();
    public required string Condition { get; set; }
}
