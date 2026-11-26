using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookMemberDto
{
    public PlaybookBoardId BoardId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsRepresentative { get; set; }
    public required string Diff { get; set; }
}
