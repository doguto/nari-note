using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public List<ArticleTag> ArticleTags { get; set; } = new();
}
