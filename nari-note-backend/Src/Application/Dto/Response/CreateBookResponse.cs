using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class CreateBookResponse
{
    public BookId Id { get; set; }
    public BookPositionId RootPositionId { get; set; }
}
