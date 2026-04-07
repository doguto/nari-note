using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(UserId), nameof(CourseId), IsUnique = true)]
[Index(nameof(CourseId))]
[Index(nameof(UserId), nameof(CreatedAt))]
public class CourseLike : EntityBase
{
    [Key]
    public CourseLikeId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [ForeignKey("Course")]
    public CourseId CourseId { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Course Course { get; set; }
}
