using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

// 定跡の全体。途中の局面は、手順を指し直して手元で求める
public class GetPlaybookResponse
{
    public PlaybookId Id { get; set; }
    public required string Name { get; set; }
    public required string RootSfen { get; set; }
    public List<PlaybookNodeDto> Nodes { get; set; } = new();
    public List<PlaybookNodeBoardDto> Boards { get; set; } = new();
    public List<PlaybookNodeStepDto> Steps { get; set; } = new();
}
