using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class WithdrawRequest
{
    [Required(ErrorMessage = "パスワードは必須です")]
    public required string Password { get; set; }
}
