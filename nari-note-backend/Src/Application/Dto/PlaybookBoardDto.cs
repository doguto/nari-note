using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookBoardDto
{
    public PlaybookBoardId Id { get; set; }
    public PlaybookNodeId? NodeId { get; set; }

    // 応答を小さくするため、ノードの局面だけに入れる。ほかの局面は開始局面から遷移を指して求める
    public string? Sfen { get; set; }
}
