using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 研究の単位になる形。似た盤面をまとめると、1 つのノードに複数の盤面が属する
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
    public List<PlaybookNodeBoard> Boards { get; set; } = new();

    // 名前もメモも無く、盤面が 1 つだけのノードは、分岐や合流でなければ手順の途中に戻してよい
    public bool HasContent => this.Name.Length > 0 || this.Memo.Length > 0 || this.Boards.Count > 1;
}
