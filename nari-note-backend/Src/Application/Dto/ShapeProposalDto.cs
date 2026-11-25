using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class ShapeProposalDto
{
    public ShapeCandidateId CandidateId { get; set; }
    public BookPositionId OtherPositionId { get; set; }
    public required string OtherName { get; set; }
    public required string Verdict { get; set; }
    public int? Delta { get; set; }
    public int? ConfirmedDepth { get; set; }
    public required string Diff { get; set; }
}
