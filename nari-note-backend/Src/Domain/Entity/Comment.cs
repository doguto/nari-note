using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(ArticleId))]
[Index(nameof(UserId))]
[Index(nameof(ArticleId), nameof(CreatedAt))]
public class Comment : EntityBase
{
    [Key]
    public CommentId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public ArticleId ArticleId { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Message { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Article Article { get; set; }
}
