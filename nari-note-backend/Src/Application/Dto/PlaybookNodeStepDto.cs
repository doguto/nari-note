using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookNodeStepDto
{
    public PlaybookNodeStepId Id { get; set; }
    public PlaybookNodeBoardId FromBoardId { get; set; }
    public PlaybookNodeBoardId? ToBoardId { get; set; }
    public List<string> Usis { get; set; } = new();
    public required string Condition { get; set; }
}
