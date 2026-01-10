using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(Name), IsUnique = true)]
public class Tag: EntityBase
{
    [Key]
    public TagId Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    // Navigation Properties
    public List<ArticleTag> ArticleTags { get; set; } = new();
}
