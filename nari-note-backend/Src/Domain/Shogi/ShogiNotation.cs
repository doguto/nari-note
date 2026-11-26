namespace NariNoteBackend.Domain.Shogi;

public static class ShogiNotation
{
    const string FullWidthDigits = "０１２３４５６７８９";
    const string KanjiDigits = "〇一二三四五六七八九";

    public static string PieceName(PieceKind kind, bool promoted) => (kind, promoted) switch
    {
        (PieceKind.Pawn, true) => "と",
        (PieceKind.Lance, true) => "成香",
        (PieceKind.Knight, true) => "成桂",
        (PieceKind.Silver, true) => "成銀",
        (PieceKind.Bishop, true) => "馬",
        (PieceKind.Rook, true) => "龍",
        (PieceKind.Pawn, _) => "歩",
        (PieceKind.Lance, _) => "香",
        (PieceKind.Knight, _) => "桂",
        (PieceKind.Silver, _) => "銀",
        (PieceKind.Gold, _) => "金",
        (PieceKind.Bishop, _) => "角",
        (PieceKind.Rook, _) => "飛",
        _ => "玉",
    };

    public static string SideMark(Side side) => side == Side.Sente ? "▲" : "△";

    public static string SquareLabel(int square) => $"{FullWidthDigits[square / 9 + 1]}{KanjiDigits[square % 9 + 1]}";

    // 曖昧な指し手は KIF と同じく移動元を括弧で添える
    public static string MoveLabel(ShogiBoard position, ShogiMove move, int? previousTo = null)
    {
        var mark = SideMark(position.Turn);
        var destination = previousTo == move.To ? "同　" : SquareLabel(move.To);
        var legal = position.LegalMoves();

        if (move.Drop is { } kind)
        {
            var boardAlternative = legal.Any(m =>
                !m.IsDrop && m.To == move.To && position[m.From!.Value] is { } p && p.Kind == kind && !p.Promoted
            );
            return $"{mark}{destination}{PieceName(kind, false)}{(boardAlternative ? "打" : "")}";
        }

        var piece = position[move.From!.Value]!.Value;
        var label = $"{mark}{destination}{PieceName(piece.Kind, piece.Promoted)}";
        var ambiguous = legal.Any(m => !m.IsDrop && m.To == move.To && m.From != move.From && position[m.From!.Value] == piece);
        if (ambiguous) label += $"({move.From.Value / 9 + 1}{move.From.Value % 9 + 1})";
        if (move.Promote) return label + "成";
        if (legal.Contains(move with { Promote = true })) return label + "不成";
        return label;
    }

    public static List<string> SequenceLabels(string sfen, IEnumerable<string> usiMoves)
    {
        var labels = new List<string>();
        var position = ShogiBoard.FromSfen(sfen);
        int? previousTo = null;
        foreach (var usi in usiMoves)
        {
            ShogiMove move;
            try
            {
                move = ShogiMove.Parse(usi);
            }
            catch (ArgumentException)
            {
                break;
            }
            if (!position.IsLegal(move)) break;
            labels.Add(MoveLabel(position, move, previousTo));
            position = position.Apply(move);
            previousTo = move.To;
        }
        return labels;
    }
}
