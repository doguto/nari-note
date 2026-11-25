using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(BookId), nameof(Sfen), IsUnique = true)]
[Index(nameof(CandidatesScanned))]
public class BookPosition : EntityBase
{
    public const int SfenMaxLength = 200;

    [Key]
    public BookPositionId Id { get; set; }

    [Required]
    public BookId BookId { get; set; }

    // 手数を 1 に正規化した SFEN
    [Required]
    [MaxLength(SfenMaxLength)]
    public required string Sfen { get; set; }

    [Required]
    public ShapeGroupId ShapeGroupId { get; set; }

    // 同じ形の候補を探し終えたか。バックグラウンドジョブが探す
    public bool CandidatesScanned { get; set; }

    // Navigation Properties
    public Book Book { get; set; } = null!;
    public ShapeGroup ShapeGroup { get; set; } = null!;
}
