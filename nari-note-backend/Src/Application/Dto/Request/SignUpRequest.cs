using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SignUpRequest
{
    [Required(ErrorMessage = "ユーザー名は必須です")]
    [MaxLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    [MaxLength(255, ErrorMessage = "メールアドレスは255文字以内で入力してください")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "パスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [MaxLength(255, ErrorMessage = "パスワードは255文字以内で入力してください")]
    public string Password { get; set; } = string.Empty;
}
