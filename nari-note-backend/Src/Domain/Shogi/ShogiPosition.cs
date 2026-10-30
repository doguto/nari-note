namespace NariNoteBackend.Domain.Shogi;

public enum PieceOwner
{
    Sente,
    Gote
}

public record ShogiPiece(string Type, PieceOwner Owner)
{
    public static readonly IReadOnlySet<string> PromotedTypes = new HashSet<string> { "龍", "馬", "成銀", "成桂", "成香", "と" };

    public bool IsPromoted => PromotedTypes.Contains(this.Type);
}

public class CapturedPiece
{
    public required string Type { get; init; }
    public int Count { get; set; }
}

/// <summary>
///     局面。Board[row, col] の row 0 は一段目、col 0 は9筋
/// </summary>
public class ShogiPosition
{
    public const int Size = 9;

    public ShogiPiece?[,] Board { get; } = new ShogiPiece?[Size, Size];
    public List<CapturedPiece> SenteCaptured { get; } = new();
    public List<CapturedPiece> GoteCaptured { get; } = new();

    public bool HasAnyPiece => this.Board.Cast<ShogiPiece?>().Any(p => p != null);

    public List<CapturedPiece> CapturedOf(PieceOwner owner)
    {
        return owner == PieceOwner.Sente ? this.SenteCaptured : this.GoteCaptured;
    }
}
