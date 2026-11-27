using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetPlaybookResponse
{
    public PlaybookId Id { get; set; }
    public required string Name { get; set; }
    public required string RootSfen { get; set; }
    public List<PlaybookNodeDto> Nodes { get; set; } = new();
    public List<PlaybookBoardDto> Boards { get; set; } = new();
    public List<PlaybookBoardTransitionDto> Transitions { get; set; } = new();
}
