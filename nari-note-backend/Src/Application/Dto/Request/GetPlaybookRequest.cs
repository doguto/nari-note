using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetPlaybookRequest
{
    public required PlaybookId Id { get; set; }
}
