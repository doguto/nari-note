using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(Token), IsUnique = true)]
public class EmailVerification : EntityBase
{
    [Key]
    public EmailVerificationId Id { get; set; }

    public UserId UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    // Navigation
    public User User { get; set; } = null!;
}
