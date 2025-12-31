using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Application.Validation;

namespace NariNoteBackend.Application.Dto.Request;

public class SignUpRequest
{
    [Required(ErrorMessage = "名前は必須です")]
    [MaxLength(50, ErrorMessage = "名前は50文字以内で入力してください")]
    public required string Name { get; set; }
    
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    [MaxLength(255, ErrorMessage = "メールアドレスは255文字以内で入力してください")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "パスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [MaxLength(255, ErrorMessage = "パスワードは255文字以内で入力してください")]
    [StrongPassword]
    public required string Password { get; set; }
}
