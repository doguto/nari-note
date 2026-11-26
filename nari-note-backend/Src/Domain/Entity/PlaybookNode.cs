using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 名前と研究メモを付ける単位。似た局面をまとめると、1 つのノードに複数の局面が属する
[Index(nameof(PlaybookId))]
public class PlaybookNode : EntityBase
{
    public const int NameMaxLength = 100;
    public const int MemoMaxLength = 2000;

    [Key]
    public PlaybookNodeId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = "";

    [MaxLength(MemoMaxLength)]
    public string Memo { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public List<PlaybookBoard> Boards { get; set; } = new();

    // 名前もメモも無く、局面が 1 つだけのノードは持っておく意味が無い
    public bool HasContent => this.Name.Length > 0 || this.Memo.Length > 0 || this.Boards.Count > 1;
}
