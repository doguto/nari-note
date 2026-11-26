using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class SplitShapeGroupRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookPositionId PositionId { get; set; }
}
