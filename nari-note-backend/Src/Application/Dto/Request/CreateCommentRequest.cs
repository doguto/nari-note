using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class CreateCommentRequest
{
    [Required(ErrorMessage = "記事IDは必須です")]
    public ArticleId ArticleId { get; set; }

    [Required(ErrorMessage = "メッセージは必須です")]
    [MaxLength(1000, ErrorMessage = "メッセージは1000文字以内で入力してください")]
    public string Message { get; set; } = string.Empty;
}
