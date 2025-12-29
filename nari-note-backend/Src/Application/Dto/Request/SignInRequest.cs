using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SignInRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "パスワードは必須です")]
    public required string Password { get; set; }
}
