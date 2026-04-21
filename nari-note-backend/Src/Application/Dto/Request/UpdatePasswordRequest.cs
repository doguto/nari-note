using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdatePasswordRequest
{
    [Required(ErrorMessage = "現在のパスワードは必須です")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [MaxLength(255, ErrorMessage = "パスワードは255文字以内で入力してください")]
    public required string NewPassword { get; set; }
}
