using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SignInRequest
{
    [Required(ErrorMessage = "ユーザー名またはメールアドレスは必須です")]
    public required string UsernameOrEmail { get; set; }
    
    [Required(ErrorMessage = "パスワードは必須です")]
    public required string Password { get; set; }
}
