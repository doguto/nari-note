using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeletePlaybookMoveRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookMoveId MoveId { get; set; }
}
