using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class MergeShapeCandidateRequest
{
    public required BookId Id { get; set; }
    public required ShapeCandidateId CandidateId { get; set; }
}
