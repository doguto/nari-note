using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

// OtherBoardId の局面が属するノードを、BoardId の局面が属するノードにまとめる
public class MergePlaybookNodesRequest
{
    public PlaybookId Id { get; set; }
    public PlaybookBoardId BoardId { get; set; }
    public PlaybookBoardId OtherBoardId { get; set; }
}
