using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

// 局面について、サーバーで計算する情報。ノードでない辺の途中の局面では、名前の候補だけを返す
public class InspectPlaybookBoardResponse
{
    public PlaybookBoardId? BoardId { get; set; }
    public List<string> NameSuggestions { get; set; } = new();
    public List<PlaybookMemberDto> Members { get; set; } = new();
    public List<PlaybookSimilarBoardDto> Similar { get; set; } = new();
    public List<PlaybookConditionSuggestionDto> ConditionSuggestions { get; set; } = new();
}
