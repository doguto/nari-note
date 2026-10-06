using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SignInRequest
{
    // フィールド名はフロントエンドとの互換のため維持（値はメールアドレスとして扱う）
    [Required(ErrorMessage = "メールアドレスは必須です")]
    public required string UsernameOrEmail { get; set; }
    
    [Required(ErrorMessage = "パスワードは必須です")]
    public required string Password { get; set; }
}
