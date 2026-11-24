using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class ShapeMemberDto
{
    public BookPositionId PositionId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsRepresentative { get; set; }
    public required string Diff { get; set; }
}
