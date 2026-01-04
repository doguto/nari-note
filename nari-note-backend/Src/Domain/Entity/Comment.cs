using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain.Entity;

public class Comment : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public int ArticleId { get; set; }

    [Required]
    public required string Message { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Article Article { get; set; }
}
