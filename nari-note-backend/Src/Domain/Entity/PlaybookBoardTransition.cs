using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 局面から 1 手指した先の局面への遷移
[Index(nameof(PlaybookId))]
[Index(nameof(FromBoardId), nameof(ToBoardId), IsUnique = true)]
[Index(nameof(ToBoardId))]
public class PlaybookBoardTransition : EntityBase
{
    public const int UsiMaxLength = 8;
    public const int ConditionMaxLength = 100;

    [Key]
    public PlaybookBoardTransitionId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [Required]
    public PlaybookBoardId FromBoardId { get; set; }

    [Required]
    public PlaybookBoardId ToBoardId { get; set; }

    [Required]
    [MaxLength(UsiMaxLength)]
    public required string Usi { get; set; }

    // まとめた局面によって同じ手の行き先が分かれるときに、この遷移がどの局面の話かを書く
    [MaxLength(ConditionMaxLength)]
    public string Condition { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookBoard FromBoard { get; set; } = null!;
    public PlaybookBoard ToBoard { get; set; } = null!;
}
