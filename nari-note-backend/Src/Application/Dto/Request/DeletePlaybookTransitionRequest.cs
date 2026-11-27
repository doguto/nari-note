using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeletePlaybookTransitionRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookBoardTransitionId TransitionId { get; set; }
}
