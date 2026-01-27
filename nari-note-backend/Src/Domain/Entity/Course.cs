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

    // Navigation Properties
    public User User { get; set; }
    public List<CourseLike> CourseLikes { get; set; } = new();
    public List<Article> Articles { get; set; } = new();
    
    public int LikeCount => CourseLikes.Count;

    // Domain Logic
    public bool IsLikedBy(UserId userId)
    {
        return CourseLikes.Any(l => l.UserId == userId);
    }
}
