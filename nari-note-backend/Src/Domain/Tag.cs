using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(Name), IsUnique = true)]
public class Tag: EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    // Navigation Properties
    public List<ArticleTag> ArticleTags { get; set; } = new();
}
