using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(PlaybookId), nameof(Sfen), IsUnique = true)]
public class PlaybookNodeBoard : EntityBase
{
    public const int SfenMaxLength = 200;

    [Key]
    public PlaybookNodeBoardId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [Required]
    public PlaybookNodeId NodeId { get; set; }

    // 手数を 1 に正規化した SFEN
    [Required]
    [MaxLength(SfenMaxLength)]
    public required string Sfen { get; set; }

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookNode Node { get; set; } = null!;
}
