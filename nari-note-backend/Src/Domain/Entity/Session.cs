using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(SessionKey), IsUnique = true)]
public class Session : EntityBase
{
    [Key]
    public SessionId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string SessionKey { get; set; }

    public DateTime ExpiresAt { get; set; }

    public Session()
    {
        ExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    // Navigation Properties
    public required User User { get; set; }
}
