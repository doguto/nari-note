using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "トークンは必須です")]
    public required string Token { get; set; }

    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [MaxLength(100, ErrorMessage = "パスワードは100文字以内で入力してください")]
    public required string NewPassword { get; set; }
}
