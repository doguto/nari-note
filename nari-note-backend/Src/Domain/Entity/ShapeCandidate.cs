using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

public enum ShapeCandidateStatus
{
    Queued,
    Running,
    Done,
    Failed,
}

public enum ShapeCandidateDecision
{
    Pending,
    Merged,
    Rejected,
}

// 同じ形かもしれない 2 局面。エンジンで判定し、まとめるかどうかは本人が決める
[Index(nameof(PositionAId), nameof(PositionBId), IsUnique = true)]
[Index(nameof(Status))]
public class ShapeCandidate : EntityBase
{
    public const string VerdictSame = "same";
    public const string VerdictSameWithDelta = "same_with_delta";
    public const string VerdictSameUntil = "same_until";
    public const string VerdictDifferent = "different";
    public const string VerdictSkipped = "skipped";

    [Key]
    public ShapeCandidateId Id { get; set; }

    [Required]
    public BookId BookId { get; set; }

    [Required]
    public BookPositionId PositionAId { get; set; }

    [Required]
    public BookPositionId PositionBId { get; set; }

    public ShapeCandidateStatus Status { get; set; } = ShapeCandidateStatus.Queued;

    [MaxLength(32)]
    public string? Verdict { get; set; }

    // 評価値の差（A − B、A の手番側から見た値）
    public int? Delta { get; set; }

    public int? ConfirmedDepth { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public ShapeCandidateDecision Decision { get; set; } = ShapeCandidateDecision.Pending;

    // Navigation Properties
    public Book Book { get; set; } = null!;
    public BookPosition PositionA { get; set; } = null!;
    public BookPosition PositionB { get; set; } = null!;

    public bool IsProposal => Status == ShapeCandidateStatus.Done
        && Decision == ShapeCandidateDecision.Pending
        && Verdict is VerdictSame or VerdictSameWithDelta or VerdictSameUntil;
}
