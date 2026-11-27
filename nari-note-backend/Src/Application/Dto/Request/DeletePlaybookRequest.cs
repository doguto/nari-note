using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeletePlaybookRequest
{
    public required PlaybookId Id { get; set; }
}
