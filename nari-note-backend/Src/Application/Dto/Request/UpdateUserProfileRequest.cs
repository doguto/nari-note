using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateUserProfileRequest
{
    [MaxLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\u3041-\u30FF\u4E00-\u9FFF]+$", ErrorMessage = "ユーザー名は全角文字・半角英数字・アンダーバー・ハイフンのみ使用可能です")]
    public string? Name { get; set; }

    [MaxLength(500, ErrorMessage = "自己紹介は500文字以内で入力してください")]
    public string? Bio { get; set; }

    // 空文字列は紐付け解除として扱う
    [MaxLength(15, ErrorMessage = "Xのユーザー名は15文字以内で入力してください")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Xのユーザー名は半角英数字・アンダーバーのみ使用可能です")]
    public string? XAccountName { get; set; }
}
