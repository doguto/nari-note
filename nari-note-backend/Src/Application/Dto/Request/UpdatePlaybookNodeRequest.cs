using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

// 局面で指定する。まだノードでない手順の途中の局面なら、そこをノードにする
public class UpdatePlaybookNodeRequest
{
    public PlaybookId Id { get; set; }

    [Required(ErrorMessage = "局面は必須です")]
    [MaxLength(PlaybookNodeBoard.SfenMaxLength)]
    public required string Sfen { get; set; }

    [MaxLength(PlaybookNode.NameMaxLength, ErrorMessage = "形の名前は100文字以内で入力してください")]
    public string? Name { get; set; }

    [MaxLength(PlaybookNode.MemoMaxLength, ErrorMessage = "研究メモは2000文字以内で入力してください")]
    public string? Memo { get; set; }
}
