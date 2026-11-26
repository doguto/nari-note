using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(UserId))]
public class Playbook : EntityBase
{
    [Key]
    public PlaybookId Id { get; set; }

    [Required]
    public required UserId UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(PlaybookNodeBoard.SfenMaxLength)]
    public required string RootSfen { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public List<PlaybookNode> Nodes { get; set; } = new();
    public List<PlaybookNodeBoard> Boards { get; set; } = new();
    public List<PlaybookNodeStep> Steps { get; set; } = new();

    public bool IsOwnedBy(UserId userId)
    {
        return UserId == userId;
    }
}
