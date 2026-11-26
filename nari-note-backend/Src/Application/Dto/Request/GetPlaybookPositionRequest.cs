using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetPlaybookPositionRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookPositionId PositionId { get; set; }
}
