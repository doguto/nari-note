using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

public class Notification : EntityBase
{
    [Key]
    public NotificationId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public ArticleId ArticleId { get; set; }

    public bool IsRead { get; set; } = false;

    // Navigation Properties
    public User User { get; set; }
    public Article Article { get; set; }
}
