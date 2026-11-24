using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeleteBookRequest
{
    public required BookId Id { get; set; }
}
