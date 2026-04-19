using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    public required string Email { get; set; }
}
