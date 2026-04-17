using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

public class Kifu : EntityBase
{
    [Key]
    public KifuId Id { get; set; }

    [Required]
    public required ArticleId ArticleId { get; set; }

    [Required]
    [MaxLength(4096)]
    public required string KifuText { get; set; }

    [Required]
    public required int SortOrder { get; set; }

    // Navigation Properties
    public Article Article { get; set; }
}
