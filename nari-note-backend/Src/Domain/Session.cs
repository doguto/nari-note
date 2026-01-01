using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(SessionKey), IsUnique = true)]
public class Session : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

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
