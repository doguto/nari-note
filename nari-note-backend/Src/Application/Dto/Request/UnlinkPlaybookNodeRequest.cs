using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UnlinkPlaybookNodeRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookNodeId NodeId { get; set; }
}
