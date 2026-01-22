using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(AuthorId))]
[Index(nameof(CreatedAt))]
[Index(nameof(IsPublished), nameof(PublishedAt), nameof(CreatedAt))]
public class Article : EntityBase
{
    [Key]
    public ArticleId Id { get; set; }

    [Required]
    [ForeignKey("Author")]
    public UserId AuthorId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public required string Body { get; set; }

    public bool IsPublished { get; set; } = false;
    
    public DateTime? PublishedAt { get; set; }

    // Navigation Properties
    public User Author { get; set; }
    public List<ArticleTag> ArticleTags { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();

    // Domain Logic
    public bool IsLikedBy(UserId userId) => Likes.Any(l => l.UserId == userId);
    public int LikeCount => Likes.Count;
}
