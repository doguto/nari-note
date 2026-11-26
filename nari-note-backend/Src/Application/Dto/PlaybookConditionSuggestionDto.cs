using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookConditionSuggestionDto
{
    public PlaybookBoardTransitionId TransitionId { get; set; }
    public List<string> Suggestions { get; set; } = new();
}
