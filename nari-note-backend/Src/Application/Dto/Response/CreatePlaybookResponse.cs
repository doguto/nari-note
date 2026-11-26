using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class CreatePlaybookResponse
{
    public PlaybookId Id { get; set; }
    public PlaybookPositionId RootPositionId { get; set; }
}
