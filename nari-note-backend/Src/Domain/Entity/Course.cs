using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
public class Course : EntityBase
{
    [Key]
    public CourseId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public List<CourseLike> CourseLikes { get; set; } = new();
    public List<Article> Articles { get; set; } = new();
    
    public int LikeCount => CourseLikes.Count;
    public bool IsPublished => PublishedAt.HasValue;

    // 予約公開を考慮し、公開日時を過ぎているかまで判定する
    public bool IsPubliclyVisibleAt(DateTime now)
    {
        return PublishedAt.HasValue && PublishedAt.Value <= now;
    }

    // Domain Logic
    public bool IsLikedBy(UserId userId)
    {
        return CourseLikes.Any(l => l.UserId == userId);
    }
}
