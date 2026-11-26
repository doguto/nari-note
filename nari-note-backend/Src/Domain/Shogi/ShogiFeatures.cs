namespace NariNoteBackend.Domain.Shogi;

// 形の名前や辺の条件の候補を盤面から作る。候補を出すだけで、決めるのはユーザー
public static class ShogiFeatures
{
    const int MaxSuggestions = 5;
    const int MaxConditionUnits = 4;

    // 先手の配置で書き、後手は盤を 180 度回して当てはめる
    static readonly (string Name, (string Square, PieceKind Kind)[] Pieces)[] Castles =
    [
        ("居飛車穴熊", [("9i", PieceKind.King), ("9h", PieceKind.Lance), ("8h", PieceKind.Silver)]),
        ("振り飛車穴熊", [("1i", PieceKind.King), ("1h", PieceKind.Lance), ("2h", PieceKind.Silver)]),
        ("金矢倉", [("8h", PieceKind.King), ("7h", PieceKind.Gold), ("6g", PieceKind.Gold), ("7g", PieceKind.Silver)]),
        ("銀矢倉", [("8h", PieceKind.King), ("7h", PieceKind.Gold), ("6g", PieceKind.Silver), ("7g", PieceKind.Silver)]),
        ("銀冠", [("2h", PieceKind.King), ("2g", PieceKind.Silver), ("3h", PieceKind.Gold)]),
        ("高美濃", [("2h", PieceKind.King), ("3h", PieceKind.Silver), ("4i", PieceKind.Gold), ("4g", PieceKind.Gold)]),
        ("本美濃", [("2h", PieceKind.King), ("3h", PieceKind.Silver), ("4i", PieceKind.Gold), ("5h", PieceKind.Gold)]),
        ("片美濃", [("2h", PieceKind.King), ("3h", PieceKind.Silver), ("4i", PieceKind.Gold)]),
        ("舟囲い", [("7h", PieceKind.King), ("6i", PieceKind.Gold), ("5h", PieceKind.Gold)]),
        ("雁木", [("6g", PieceKind.Silver), ("4g", PieceKind.Silver), ("7h", PieceKind.Gold)]),
    ];

    public static List<string> NameSuggestions(ShogiBoard position)
    {
        if (position.ToSfen() == ShogiBoard.Normalize(ShogiBoard.Hirate)) return ["平手"];

        var opening = Opening(position);
        var formation = Formation(position);
        var edges = string.Join("・", new[] { 9, 1 }.Select(f => EdgeLabel(position, f)).OfType<string>());
        var withFormation = opening + formation;

        var suggestions = new List<string>();
        if (edges.Length > 0) suggestions.Add($"{withFormation} {edges}");
        suggestions.Add(withFormation);
        suggestions.Add(opening);

        var sente = Castle(position, Side.Sente);
        var gote = Castle(position, Side.Gote);
        if (sente is not null || gote is not null)
        {
            suggestions.Add($"{sente ?? Style(position, Side.Sente) ?? "先手"} 対 {gote ?? Style(position, Side.Gote) ?? "後手"}");
        }
        return suggestions.Distinct().Take(MaxSuggestions).ToList();
    }

    // others は同じ形にまとめた別の局面。current との違いを、current 側の状態として言葉にする
    public static List<string> ConditionSuggestions(ShogiBoard current, IEnumerable<ShogiBoard> others)
    {
        var suggestions = new List<string>();
        foreach (var other in others)
        {
            var units = ShogiDiff.Units(current, other);
            if (units.Count == 0 || units.Count > MaxConditionUnits) continue;

            var edgeFiles = units.Select(EdgePawnFile).ToList();
            if (edgeFiles.All(f => f.HasValue))
            {
                suggestions.Add(string.Join("、", edgeFiles.Distinct().Select(f => EdgeCondition(current, f!.Value))));
            }
            suggestions.Add(string.Join("、", units.Select(u => $"{ShogiNotation.SideMark(u.From.Side)}{ShogiNotation.PieceName(u.From.Kind, u.From.Promoted)}が{Where(u.From)}")));
        }
        return suggestions.Distinct().Take(MaxSuggestions).ToList();
    }

    public static string? Style(ShogiBoard position, Side side)
    {
        var rook = Find(position, side, PieceKind.Rook, false).Cast<int?>().FirstOrDefault();
        if (rook is null) return null;
        var file = side == Side.Sente ? rook.Value / 9 + 1 : 9 - rook.Value / 9;
        return file switch
        {
            <= 3 => "居飛車",
            4 => "右四間飛車",
            5 => "中飛車",
            6 => "四間飛車",
            7 => "三間飛車",
            8 => "向かい飛車",
            _ => null,
        };
    }

    public static string? Castle(ShogiBoard position, Side side)
    {
        foreach (var (name, pieces) in Castles)
        {
            if (pieces.All(p => Has(position, Oriented(p.Square, side), side, p.Kind))) return name;
        }
        return null;
    }

    static string Opening(ShogiBoard position)
    {
        var sente = Style(position, Side.Sente);
        var gote = Style(position, Side.Gote);
        var senteStatic = sente is null or "居飛車" or "右四間飛車";
        var goteStatic = gote is null or "居飛車" or "右四間飛車";
        if (senteStatic && goteStatic) return IsBishopExchanged(position) ? "角換わり" : "相居飛車";
        if (!senteStatic && !goteStatic) return "相振り飛車";
        return senteStatic ? $"後手{gote}" : $"先手{sente}";
    }

    static string Formation(ShogiBoard position)
    {
        if (Has(position, ShogiBoard.ParseSquare("5f"), Side.Sente, PieceKind.Silver)
            || Has(position, ShogiBoard.ParseSquare("5d"), Side.Gote, PieceKind.Silver)) return "腰掛け銀";
        if (Style(position, Side.Sente) != "居飛車") return "";
        if (Has(position, ShogiBoard.ParseSquare("4f"), Side.Sente, PieceKind.Silver)) return "早繰り銀";
        if (Has(position, ShogiBoard.ParseSquare("2f"), Side.Sente, PieceKind.Silver)
            || Has(position, ShogiBoard.ParseSquare("2e"), Side.Sente, PieceKind.Silver)) return "棒銀";
        return "";
    }

    static bool IsBishopExchanged(ShogiBoard position) =>
        position.HandCount(Side.Sente, PieceKind.Bishop) + position.HandCount(Side.Gote, PieceKind.Bishop) > 0;

    static string? EdgeLabel(ShogiBoard position, int file) => EdgeState(position, file) switch
    {
        (true, true) => $"{file}筋突き合い",
        (true, false) => $"{file}筋先手だけ",
        (false, true) => $"{file}筋後手だけ",
        _ => null,
    };

    static string EdgeCondition(ShogiBoard position, int file) => EdgeState(position, file) switch
    {
        (true, true) => $"{file}筋を突き合っている",
        (true, false) => $"{file}筋を先手だけ突いている",
        (false, true) => $"{file}筋を後手だけ突いている",
        _ => $"{file}筋を突いていない",
    };

    static (bool Sente, bool Gote) EdgeState(ShogiBoard position, int file)
    {
        var sente = Find(position, Side.Sente, PieceKind.Pawn, false).Where(sq => sq / 9 + 1 == file).ToList();
        var gote = Find(position, Side.Gote, PieceKind.Pawn, false).Where(sq => sq / 9 + 1 == file).ToList();
        return (sente.Any(sq => sq % 9 < 6), gote.Any(sq => sq % 9 > 2));
    }

    static int? EdgePawnFile(DiffUnit unit)
    {
        if (unit.From.Kind != PieceKind.Pawn || unit.From.Side != unit.To.Side) return null;
        if (unit.From.Square is not { } from || unit.To.Square is not { } to) return null;
        var file = from / 9 + 1;
        return file is 1 or 9 && to / 9 + 1 == file ? file : null;
    }

    static string Where(PieceLocation location) =>
        location.Square is { } sq ? ShogiNotation.SquareLabel(sq) : "持駒";

    static int Oriented(string square, Side side)
    {
        var sq = ShogiBoard.ParseSquare(square);
        return side == Side.Sente ? sq : 80 - sq;
    }

    static bool Has(ShogiBoard position, int square, Side side, PieceKind kind) =>
        position[square] is { } p && p.Side == side && p.Kind == kind && !p.Promoted;

    static IEnumerable<int> Find(ShogiBoard position, Side side, PieceKind kind, bool promoted)
    {
        for (var sq = 0; sq < 81; sq++)
        {
            if (position[sq] is { } p && p.Side == side && p.Kind == kind && p.Promoted == promoted) yield return sq;
        }
    }
}
