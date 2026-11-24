using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetBookRequest
{
    public required BookId Id { get; set; }
}
