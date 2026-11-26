using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetPlaybookResponse
{
    public PlaybookId Id { get; set; }
    public required string Name { get; set; }
    public PlaybookPositionId RootPositionId { get; set; }
}
