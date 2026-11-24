using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeleteBookMoveRequest
{
    public required BookId Id { get; set; }
    public required BookMoveId MoveId { get; set; }
}
