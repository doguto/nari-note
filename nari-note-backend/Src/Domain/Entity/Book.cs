using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(UserId))]
public class Book : EntityBase
{
    [Key]
    public BookId Id { get; set; }

    [Required]
    public required UserId UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(BookPosition.SfenMaxLength)]
    public required string RootSfen { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public List<BookPosition> Positions { get; set; } = new();
    public List<BookMove> Moves { get; set; } = new();
    public List<ShapeGroup> ShapeGroups { get; set; } = new();
    public List<ShapeCandidate> ShapeCandidates { get; set; } = new();

    public bool IsOwnedBy(UserId userId)
    {
        return UserId == userId;
    }
}
