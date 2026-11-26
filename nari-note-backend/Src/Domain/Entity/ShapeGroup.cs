using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 本質的に同じ形としてまとめた局面の集まり。まとめていない局面も 1 局面だけのグループに属する
[Index(nameof(PlaybookId))]
public class ShapeGroup : EntityBase
{
    [Key]
    public ShapeGroupId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(10000)]
    public string Memo { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public List<PlaybookPosition> Positions { get; set; } = new();
}
