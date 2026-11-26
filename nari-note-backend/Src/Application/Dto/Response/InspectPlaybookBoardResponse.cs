using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

// 局面について、サーバーで計算する情報。ノードでない局面では盤面に関わる項目は空になる
public class InspectPlaybookBoardResponse
{
    public PlaybookNodeBoardId? BoardId { get; set; }
    public List<string> NameSuggestions { get; set; } = new();
    public List<PlaybookMemberDto> Members { get; set; } = new();
    public List<PlaybookSimilarBoardDto> Similar { get; set; } = new();
    public List<PlaybookConditionSuggestionDto> ConditionSuggestions { get; set; } = new();
}
