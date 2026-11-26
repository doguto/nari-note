using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 定跡に現れる局面。指した局面はすべて 1 行になり、手順前後で同じ局面に着けば同じ行になる
[Index(nameof(PlaybookId), nameof(Sfen), IsUnique = true)]
[Index(nameof(NodeId))]
public class PlaybookBoard : EntityBase
{
    public const int SfenMaxLength = 200;

    [Key]
    public PlaybookBoardId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    // 名前やメモを付けた局面と、まとめた局面だけがノードに属する
    public PlaybookNodeId? NodeId { get; set; }

    // 手数を 1 に正規化した SFEN
    [Required]
    [MaxLength(SfenMaxLength)]
    public required string Sfen { get; set; }

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookNode? Node { get; set; }
}
