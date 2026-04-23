using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateUserProfileRequest
{
    [MaxLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\u3041-\u30FF\u4E00-\u9FFF]+$", ErrorMessage = "ユーザー名は全角文字・半角英数字・アンダーバー・ハイフンのみ使用可能です")]
    public string? Name { get; set; }

    [MaxLength(500, ErrorMessage = "自己紹介は500文字以内で入力してください")]
    public string? Bio { get; set; }
}
