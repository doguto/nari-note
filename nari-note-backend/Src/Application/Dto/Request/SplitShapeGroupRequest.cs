using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class SplitShapeGroupRequest
{
    public required BookId Id { get; set; }
    public required BookPositionId PositionId { get; set; }
}
