using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(ParentPositionId), nameof(Usi), IsUnique = true)]
public class PlaybookMove : EntityBase
{
    [Key]
    public PlaybookMoveId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [Required]
    public PlaybookPositionId ParentPositionId { get; set; }

    [Required]
    public PlaybookPositionId ChildPositionId { get; set; }

    [Required]
    [MaxLength(8)]
    public required string Usi { get; set; }

    // 直前の手を考慮しない記譜（▲７六歩）。グラフ表示のたびに合法手を生成しないよう保存しておく
    [Required]
    [MaxLength(32)]
    public required string Label { get; set; }

    [MaxLength(200)]
    public string Memo { get; set; } = "";

    // まとめた局面によって行き先が分かれるときに、この手がどの局面の話かを書く
    [MaxLength(100)]
    public string Condition { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookPosition ParentPosition { get; set; } = null!;
    public PlaybookPosition ChildPosition { get; set; } = null!;
}
