using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookBoardTransitionDto
{
    public PlaybookBoardTransitionId Id { get; set; }
    public PlaybookBoardId FromBoardId { get; set; }
    public PlaybookBoardId ToBoardId { get; set; }
    public required string Usi { get; set; }
    public required string Condition { get; set; }
}
