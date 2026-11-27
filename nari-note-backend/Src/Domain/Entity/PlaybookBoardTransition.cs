using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// ノードの局面から次のノードの局面までの手順。グラフの辺にあたる
[Index(nameof(PlaybookId))]
[Index(nameof(FromBoardId))]
[Index(nameof(ToBoardId))]
public class PlaybookBoardTransition : EntityBase
{
    public const int ConditionMaxLength = 100;

    [Key]
    public PlaybookBoardTransitionId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [Required]
    public PlaybookBoardId FromBoardId { get; set; }

    // ノードに着かず途中で止まる手順は null
    public PlaybookBoardId? ToBoardId { get; set; }

    [Required]
    public List<string> Usis { get; set; } = new();

    // まとめた局面によって同じ手の行き先が分かれるときに、この辺がどの局面の話かを書く
    [MaxLength(ConditionMaxLength)]
    public string Condition { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookBoard FromBoard { get; set; } = null!;
    public PlaybookBoard? ToBoard { get; set; }
}
