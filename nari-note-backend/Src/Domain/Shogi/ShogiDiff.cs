namespace NariNoteBackend.Domain.Shogi;

public readonly record struct PieceLocation(PieceKind Kind, Side Side, int? Square, bool Promoted);

public readonly record struct DiffUnit(PieceLocation From, PieceLocation To)
{
    public string Label()
    {
        if (this.From.Side == this.To.Side)
            return $"{ShogiNotation.SideMark(this.To.Side)}{ShogiNotation.PieceName(this.To.Kind, this.To.Promoted)} {Where(this.From)}→{Where(this.To)}";
        return $"{Describe(this.From)} → {Describe(this.To)}";
    }

    static string Where(PieceLocation l) => l.Square.HasValue ? ShogiNotation.SquareLabel(l.Square.Value) : "持駒";

    static string Describe(PieceLocation l) => $"{ShogiNotation.SideMark(l.Side)}{ShogiNotation.PieceName(l.Kind, l.Promoted)} {Where(l)}";
}

// 駒は常に 40 枚そろっているので、片方の局面にしかない駒を同じ種類同士で対応づけ、1 組を差分の 1 単位とする
public static class ShogiDiff
{
    public static List<DiffUnit> Units(ShogiBoard a, ShogiBoard b)
    {
        var onlyB = Locations(b);
        var onlyA = new List<PieceLocation>();
        foreach (var l in Locations(a))
        {
            if (!onlyB.Remove(l)) onlyA.Add(l);
        }

        var units = new List<DiffUnit>();
        foreach (var kind in Enum.GetValues<PieceKind>())
        {
            var src = onlyA.Where(l => l.Kind == kind).ToList();
            var dst = onlyB.Where(l => l.Kind == kind).ToList();
            while (src.Count > 0 && dst.Count > 0)
            {
                var (s, d) = src.SelectMany(x => dst.Select(y => (x, y))).MinBy(p => Cost(p.x, p.y));
                units.Add(new DiffUnit(s, d));
                src.Remove(s);
                dst.Remove(d);
            }
        }
        return units;
    }

    public static string Describe(ShogiBoard a, ShogiBoard b, int maxUnits = 4)
    {
        var units = Units(a, b);
        if (units.Count == 0) return "";
        if (units.Count > maxUnits) return $"差分 {units.Count} 単位";
        return string.Join("、", units.Select(u => u.Label()));
    }

    static List<PieceLocation> Locations(ShogiBoard position)
    {
        var result = new List<PieceLocation>();
        for (var sq = 0; sq < 81; sq++)
        {
            if (position[sq] is { } p) result.Add(new PieceLocation(p.Kind, p.Side, sq, p.Promoted));
        }
        foreach (var side in new[] { Side.Sente, Side.Gote })
        {
            foreach (var kind in Enum.GetValues<PieceKind>())
            {
                if (kind == PieceKind.King) continue;
                for (var i = 0; i < position.HandCount(side, kind); i++) result.Add(new PieceLocation(kind, side, null, false));
            }
        }
        return result;
    }

    static int Cost(PieceLocation a, PieceLocation b)
    {
        var cost = a.Side == b.Side ? 0 : 100;
        if (!a.Square.HasValue || !b.Square.HasValue) return cost + (a.Square == b.Square ? 0 : 20);
        return cost + Math.Abs(a.Square.Value / 9 - b.Square.Value / 9) + Math.Abs(a.Square.Value % 9 - b.Square.Value % 9);
    }
}
