using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class VerifyEmailRequest
{
    [Required]
    public required string Token { get; set; }
}
