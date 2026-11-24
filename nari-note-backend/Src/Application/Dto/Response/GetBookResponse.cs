using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetBookResponse
{
    public BookId Id { get; set; }
    public required string Name { get; set; }
    public BookPositionId RootPositionId { get; set; }
}
