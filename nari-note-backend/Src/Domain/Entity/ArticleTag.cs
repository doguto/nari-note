using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(ArticleId), nameof(TagId), IsUnique = true)]
public class ArticleTag : EntityBase
{
    [Key]
    public ArticleTagId Id { get; set; }

    [Required]
    [ForeignKey("Article")]
    public ArticleId ArticleId { get; set; }

    [Required]
    [ForeignKey("Tag")]
    public TagId TagId { get; set; }

    // Navigation Properties
    public required Article Article { get; set; }
    public required Tag Tag { get; set; }
}
