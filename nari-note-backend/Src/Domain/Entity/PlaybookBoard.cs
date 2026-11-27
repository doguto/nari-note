using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// ノードの局面。開始局面・分岐点・合流点と、ユーザーがノードにした局面だけが行になり、その間の局面は遷移の手順から求める
[Index(nameof(PlaybookId), nameof(Sfen), IsUnique = true)]
[Index(nameof(NodeId))]
public class PlaybookBoard : EntityBase
{
    public const int SfenMaxLength = 200;

    [Key]
    public PlaybookBoardId Id { get; set; }

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
