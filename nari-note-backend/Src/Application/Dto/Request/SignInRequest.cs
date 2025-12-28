using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SignInRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "パスワードは必須です")]
    public string Password { get; set; } = string.Empty;
}
