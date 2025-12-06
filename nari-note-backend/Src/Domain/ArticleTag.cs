using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(ArticleId), nameof(TagId), IsUnique = true)]
public class ArticleTag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Article")]
    public int ArticleId { get; set; }

    [Required]
    [ForeignKey("Tag")]
    public int TagId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public required Article Article { get; set; }
    public required Tag Tag { get; set; }
}
