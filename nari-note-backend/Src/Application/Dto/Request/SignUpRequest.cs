using System.ComponentModel.DataAnnotations;

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
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+\[\]{};':""\\|,.<>\/?]).+$",
        ErrorMessage = "パスワードは英大文字・英小文字・数字・記号をそれぞれ1文字以上含む必要があります")]
    public required string Password { get; set; }
}
