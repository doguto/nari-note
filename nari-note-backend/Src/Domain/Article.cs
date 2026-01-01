using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain;

public class Article : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Author")]
    public int AuthorId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public required string Body { get; set; }

    public bool IsPublished { get; set; } = false;

    // Navigation Properties
    public User Author { get; set; }
    public List<ArticleTag> ArticleTags { get; set; } = new();
    public List<Like> Likes { get; set; } = new();

    // Domain Logic
    public bool IsLikedBy(int userId) => Likes.Any(l => l.UserId == userId);
    public int LikeCount => Likes.Count;
}
