using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

// 盤面から次の盤面までの手順。途中の局面は行として持たず、手順を指し直して求める
[Index(nameof(PlaybookId))]
[Index(nameof(FromBoardId))]
[Index(nameof(ToBoardId))]
public class PlaybookNodeStep : EntityBase
{
    public const int ConditionMaxLength = 100;

    [Key]
    public PlaybookNodeStepId Id { get; set; }

    [Required]
    public PlaybookId PlaybookId { get; set; }

    [Required]
    public PlaybookNodeBoardId FromBoardId { get; set; }

    // 手順の終点がノードでない（その先をまだ整理していない）ときは null
    public PlaybookNodeBoardId? ToBoardId { get; set; }

    [Required]
    public List<string> Usis { get; set; } = new();

    // まとめた盤面によって行き先が分かれるときに、この手順がどの盤面の話かを書く
    [MaxLength(ConditionMaxLength)]
    public string Condition { get; set; } = "";

    // Navigation Properties
    public Playbook Playbook { get; set; } = null!;
    public PlaybookNodeBoard FromBoard { get; set; } = null!;
    public PlaybookNodeBoard? ToBoard { get; set; }
}
