using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain.Entity;

public class Notification : EntityBase
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

    // Navigation Properties
    public required User User { get; set; }
    public required Article Article { get; set; }
}
