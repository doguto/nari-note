using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class MergeShapeCandidateRequest
{
    public required PlaybookId Id { get; set; }
    public required ShapeCandidateId CandidateId { get; set; }
}
