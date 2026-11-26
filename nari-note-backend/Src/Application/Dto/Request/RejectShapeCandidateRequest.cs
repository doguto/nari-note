using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class RejectShapeCandidateRequest
{
    public required PlaybookId Id { get; set; }
    public required ShapeCandidateId CandidateId { get; set; }
}
