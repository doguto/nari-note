using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

// OtherBoardId の盤面が属するノードを、BoardId の盤面が属するノードにまとめる
public class MergePlaybookNodesRequest
{
    public PlaybookId Id { get; set; }
    public PlaybookNodeBoardId BoardId { get; set; }
    public PlaybookNodeBoardId OtherBoardId { get; set; }
}
