using System.Text;

namespace NariNoteBackend.Domain.Shogi;

public enum Side
{
    Sente,
    Gote,
}

public enum PieceKind
{
    Pawn,
    Lance,
    Knight,
    Silver,
    Gold,
    Bishop,
    Rook,
    King,
}

public readonly record struct Piece(PieceKind Kind, Side Side, bool Promoted)
{
    public bool CanPromote => !this.Promoted && this.Kind is not (PieceKind.Gold or PieceKind.King);
}

public readonly record struct ShogiMove(int? From, int To, PieceKind? Drop, bool Promote)
{
    public bool IsDrop => this.Drop.HasValue;

    public static ShogiMove Parse(string usi)
    {
        if (usi.Length >= 4 && usi[1] == '*')
        {
            var kind = ShogiBoard.KindFromLetter(char.ToUpperInvariant(usi[0]));
            return new ShogiMove(null, ShogiBoard.ParseSquare(usi.Substring(2, 2)), kind, false);
        }
        if (usi.Length is not (4 or 5) || (usi.Length == 5 && usi[4] != '+'))
            throw new ArgumentException($"指し手の形式が正しくありません: {usi}");
        return new ShogiMove(
            ShogiBoard.ParseSquare(usi.Substring(0, 2)),
            ShogiBoard.ParseSquare(usi.Substring(2, 2)),
            null,
            usi.Length == 5
        );
    }

    public string ToUsi()
    {
        var to = ShogiBoard.SquareName(this.To);
        if (this.Drop.HasValue) return $"{ShogiBoard.LetterOf(this.Drop.Value)}*{to}";
        return ShogiBoard.SquareName(this.From!.Value) + to + (this.Promote ? "+" : "");
    }
}

// マスの番号は (筋 - 1) * 9 + 段（一段目が 0）。USI の 7g は 7 筋 7 段目
public class ShogiBoard
{
    public const string Hirate = "lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1";

    static readonly PieceKind[] HandOrder =
    [
        PieceKind.Rook, PieceKind.Bishop, PieceKind.Gold, PieceKind.Silver, PieceKind.Knight, PieceKind.Lance, PieceKind.Pawn,
    ];

    static readonly (int File, int Rank)[] GoldSteps = [(0, -1), (1, -1), (-1, -1), (1, 0), (-1, 0), (0, 1)];
    static readonly (int File, int Rank)[] SilverSteps = [(0, -1), (1, -1), (-1, -1), (1, 1), (-1, 1)];
    static readonly (int File, int Rank)[] KingSteps = [(0, -1), (1, -1), (-1, -1), (1, 0), (-1, 0), (0, 1), (1, 1), (-1, 1)];
    static readonly (int File, int Rank)[] KnightSteps = [(1, -2), (-1, -2)];
    static readonly (int File, int Rank)[] Diagonals = [(1, 1), (1, -1), (-1, 1), (-1, -1)];
    static readonly (int File, int Rank)[] Orthogonals = [(0, 1), (0, -1), (1, 0), (-1, 0)];

    readonly Piece?[] board = new Piece?[81];
    readonly int[,] hands = new int[2, 7];

    public Side Turn { get; private set; }

    ShogiBoard()
    {
    }

    public Piece? this[int square] => this.board[square];

    public int HandCount(Side side, PieceKind kind) => this.hands[(int)side, (int)kind];

    public static ShogiBoard FromSfen(string sfen)
    {
        var parts = sfen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3) throw new ArgumentException("SFEN の形式が正しくありません");

        var position = new ShogiBoard();
        var rows = parts[0].Split('/');
        if (rows.Length != 9) throw new ArgumentException("SFEN の盤面は 9 段必要です");
        for (var rank = 0; rank < 9; rank++)
        {
            var file = 9;
            var promoted = false;
            foreach (var ch in rows[rank])
            {
                if (char.IsDigit(ch))
                {
                    file -= ch - '0';
                    continue;
                }
                if (ch == '+')
                {
                    promoted = true;
                    continue;
                }
                if (file < 1) throw new ArgumentException("SFEN の盤面の筋の数が正しくありません");
                var side = char.IsUpper(ch) ? Side.Sente : Side.Gote;
                position.board[Square(file, rank)] = new Piece(KindFromLetter(char.ToUpperInvariant(ch)), side, promoted);
                promoted = false;
                file--;
            }
            if (file != 0) throw new ArgumentException("SFEN の盤面の筋の数が正しくありません");
        }

        position.Turn = parts[1] switch
        {
            "b" => Side.Sente,
            "w" => Side.Gote,
            _ => throw new ArgumentException("SFEN の手番が正しくありません"),
        };

        if (parts[2] != "-")
        {
            var count = 0;
            foreach (var ch in parts[2])
            {
                if (char.IsDigit(ch))
                {
                    count = count * 10 + (ch - '0');
                    continue;
                }
                var side = char.IsUpper(ch) ? Side.Sente : Side.Gote;
                position.hands[(int)side, (int)KindFromLetter(char.ToUpperInvariant(ch))] += Math.Max(count, 1);
                count = 0;
            }
        }
        return position;
    }

    // 手数は局面の同一性に関係ないため 1 に固定する
    public string ToSfen()
    {
        var sb = new StringBuilder();
        for (var rank = 0; rank < 9; rank++)
        {
            if (rank > 0) sb.Append('/');
            var empty = 0;
            for (var file = 9; file >= 1; file--)
            {
                var piece = this.board[Square(file, rank)];
                if (piece is null)
                {
                    empty++;
                    continue;
                }
                if (empty > 0)
                {
                    sb.Append(empty);
                    empty = 0;
                }
                if (piece.Value.Promoted) sb.Append('+');
                var letter = LetterOf(piece.Value.Kind);
                sb.Append(piece.Value.Side == Side.Sente ? letter : char.ToLowerInvariant(letter));
            }
            if (empty > 0) sb.Append(empty);
        }

        sb.Append(this.Turn == Side.Sente ? " b " : " w ");
        var hand = new StringBuilder();
        foreach (var side in new[] { Side.Sente, Side.Gote })
        {
            foreach (var kind in HandOrder)
            {
                var n = this.hands[(int)side, (int)kind];
                if (n == 0) continue;
                if (n > 1) hand.Append(n);
                var letter = LetterOf(kind);
                hand.Append(side == Side.Sente ? letter : char.ToLowerInvariant(letter));
            }
        }
        sb.Append(hand.Length > 0 ? hand.ToString() : "-");
        sb.Append(" 1");
        return sb.ToString();
    }

    public static string Normalize(string sfen) => FromSfen(sfen).ToSfen();

    // 手番でない側に王手がかかっている局面は、実戦では現れず次の一手で玉が取れてしまう
    public bool IsValidTurn() => !this.IsInCheck(Opponent(this.Turn));

    public ShogiBoard Clone()
    {
        var copy = new ShogiBoard { Turn = this.Turn };
        Array.Copy(this.board, copy.board, 81);
        Array.Copy(this.hands, copy.hands, this.hands.Length);
        return copy;
    }

    public ShogiBoard Apply(ShogiMove move)
    {
        var next = this.Clone();
        if (move.Drop is { } kind)
        {
            next.hands[(int)this.Turn, (int)kind]--;
            next.board[move.To] = new Piece(kind, this.Turn, false);
        }
        else
        {
            var piece = this.board[move.From!.Value]!.Value;
            var captured = this.board[move.To];
            if (captured is not null) next.hands[(int)this.Turn, (int)captured.Value.Kind]++;
            next.board[move.From.Value] = null;
            next.board[move.To] = piece with { Promoted = piece.Promoted || move.Promote };
        }
        next.Turn = Opponent(this.Turn);
        return next;
    }

    public List<ShogiMove> LegalMoves()
    {
        var result = new List<ShogiMove>();
        foreach (var move in this.PseudoLegalMoves())
        {
            var next = this.Apply(move);
            if (next.IsInCheck(this.Turn)) continue;
            // 打ち歩詰め
            if (move.Drop == PieceKind.Pawn && next.IsInCheck(next.Turn) && !next.HasEscape()) continue;
            result.Add(move);
        }
        return result;
    }

    // 応手側の打ち歩詰めは王手の回避にならないので判定を省く
    bool HasEscape()
    {
        foreach (var move in this.PseudoLegalMoves())
        {
            if (!this.Apply(move).IsInCheck(this.Turn)) return true;
        }
        return false;
    }

    public bool IsLegal(ShogiMove move) => this.LegalMoves().Contains(move);

    public bool IsInCheck(Side side)
    {
        var king = this.KingSquare(side);
        return king.HasValue && this.IsAttacked(king.Value, Opponent(side));
    }

    int? KingSquare(Side side)
    {
        for (var sq = 0; sq < 81; sq++)
        {
            if (this.board[sq] is { Kind: PieceKind.King } p && p.Side == side) return sq;
        }
        return null;
    }

    public bool IsAttacked(int target, Side by) => this.CountAttackers(target, by) > 0;

    public int CountAttackers(int target, Side by)
    {
        var count = 0;
        for (var sq = 0; sq < 81; sq++)
        {
            if (this.board[sq] is not { } piece || piece.Side != by) continue;
            if (this.Destinations(sq, piece).Contains(target)) count++;
        }
        return count;
    }

    IEnumerable<ShogiMove> PseudoLegalMoves()
    {
        for (var sq = 0; sq < 81; sq++)
        {
            if (this.board[sq] is not { } piece || piece.Side != this.Turn) continue;
            foreach (var to in this.Destinations(sq, piece))
            {
                if (this.board[to] is { } target && (target.Side == this.Turn || target.Kind == PieceKind.King)) continue;
                var canPromote = piece.CanPromote && (InZone(sq, this.Turn) || InZone(to, this.Turn));
                if (canPromote) yield return new ShogiMove(sq, to, null, true);
                if (!MustPromote(piece, to)) yield return new ShogiMove(sq, to, null, false);
            }
        }

        for (var k = 0; k < 7; k++)
        {
            if (this.hands[(int)this.Turn, k] == 0) continue;
            var kind = (PieceKind)k;
            for (var to = 0; to < 81; to++)
            {
                if (this.board[to] is not null) continue;
                var piece = new Piece(kind, this.Turn, false);
                if (MustPromote(piece, to)) continue;
                if (kind == PieceKind.Pawn && this.HasPawnOnFile(to / 9, this.Turn)) continue;
                yield return new ShogiMove(null, to, kind, false);
            }
        }
    }

    bool HasPawnOnFile(int fileIndex, Side side)
    {
        for (var rank = 0; rank < 9; rank++)
        {
            if (this.board[fileIndex * 9 + rank] is { Kind: PieceKind.Pawn, Promoted: false } p && p.Side == side) return true;
        }
        return false;
    }

    List<int> Destinations(int from, Piece piece)
    {
        var result = new List<int>();
        var sign = piece.Side == Side.Sente ? 1 : -1;
        var file = from / 9;
        var rank = from % 9;

        void Step((int File, int Rank)[] steps)
        {
            foreach (var (df, dr) in steps)
            {
                var f = file + df;
                var r = rank + dr * sign;
                if (f is >= 0 and < 9 && r is >= 0 and < 9) result.Add(f * 9 + r);
            }
        }

        void Slide((int File, int Rank)[] dirs)
        {
            foreach (var (df, dr) in dirs)
            {
                var f = file + df;
                var r = rank + dr * sign;
                while (f is >= 0 and < 9 && r is >= 0 and < 9)
                {
                    result.Add(f * 9 + r);
                    if (this.board[f * 9 + r] is not null) break;
                    f += df;
                    r += dr * sign;
                }
            }
        }

        if (piece.Promoted && piece.Kind is PieceKind.Pawn or PieceKind.Lance or PieceKind.Knight or PieceKind.Silver)
        {
            Step(GoldSteps);
            return result;
        }

        switch (piece.Kind)
        {
            case PieceKind.Pawn:
                Step([(0, -1)]);
                break;
            case PieceKind.Lance:
                Slide([(0, -1)]);
                break;
            case PieceKind.Knight:
                Step(KnightSteps);
                break;
            case PieceKind.Silver:
                Step(SilverSteps);
                break;
            case PieceKind.Gold:
                Step(GoldSteps);
                break;
            case PieceKind.King:
                Step(KingSteps);
                break;
            case PieceKind.Bishop:
                Slide(Diagonals);
                if (piece.Promoted) Step(Orthogonals);
                break;
            case PieceKind.Rook:
                Slide(Orthogonals);
                if (piece.Promoted) Step(Diagonals);
                break;
        }
        return result;
    }

    static bool InZone(int square, Side side)
    {
        var rank = square % 9;
        return side == Side.Sente ? rank <= 2 : rank >= 6;
    }

    static bool MustPromote(Piece piece, int to)
    {
        if (piece.Promoted) return false;
        var depth = piece.Side == Side.Sente ? to % 9 : 8 - to % 9;
        return piece.Kind switch
        {
            PieceKind.Pawn or PieceKind.Lance => depth == 0,
            PieceKind.Knight => depth <= 1,
            _ => false,
        };
    }

    public static Side Opponent(Side side) => side == Side.Sente ? Side.Gote : Side.Sente;

    public static int Square(int file, int rank) => (file - 1) * 9 + rank;

    public static int ParseSquare(string name)
    {
        if (name.Length != 2 || name[0] is < '1' or > '9' || name[1] is < 'a' or > 'i')
            throw new ArgumentException($"マスの形式が正しくありません: {name}");
        return Square(name[0] - '0', name[1] - 'a');
    }

    public static string SquareName(int square) => $"{square / 9 + 1}{(char)('a' + square % 9)}";

    public static PieceKind KindFromLetter(char letter) => letter switch
    {
        'P' => PieceKind.Pawn,
        'L' => PieceKind.Lance,
        'N' => PieceKind.Knight,
        'S' => PieceKind.Silver,
        'G' => PieceKind.Gold,
        'B' => PieceKind.Bishop,
        'R' => PieceKind.Rook,
        'K' => PieceKind.King,
        _ => throw new ArgumentException($"駒の文字が正しくありません: {letter}"),
    };

    public static char LetterOf(PieceKind kind) => "PLNSGBRK"[(int)kind];
}
