using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain;

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public int ArticleId { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public Notification()
    {
        CreatedAt = DateTime.UtcNow;
    }

    // Navigation Properties
    public required User User { get; set; }
    public required Article Article { get; set; }
}
