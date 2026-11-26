namespace NariNoteBackend.Domain.Shogi;

// 似た盤面の候補。エンジンを使わず、駒割と駒の配置の違いだけで絞る。精度は未検証なので、しきい値は仮の値
public static class ShogiSimilarity
{
    public const int MaxUnits = 3;
    public const int MaxMaterialDistance = 1;

    // 似ていれば違いの単位数を、似ていなければ null を返す
    public static int? UnitCount(ShogiBoard a, ShogiBoard b)
    {
        if (a.Turn != b.Turn) return null;
        if (MaterialDistance(a, b) > MaxMaterialDistance) return null;
        var units = ShogiDiff.Units(a, b).Count;
        return units is > 0 and <= MaxUnits ? units : null;
    }

    // 駒の総数は一定なので、先手の駒の数だけを種類ごとに比べれば駒割の差が分かる
    static int MaterialDistance(ShogiBoard a, ShogiBoard b)
    {
        var ma = SenteMaterial(a);
        var mb = SenteMaterial(b);
        return ma.Zip(mb, (x, y) => Math.Abs(x - y)).Sum();
    }

    static int[] SenteMaterial(ShogiBoard board)
    {
        var counts = new int[Enum.GetValues<PieceKind>().Length];
        for (var sq = 0; sq < 81; sq++)
        {
            if (board[sq] is { Side: Side.Sente } p && p.Kind != PieceKind.King) counts[(int)p.Kind]++;
        }
        foreach (var kind in Enum.GetValues<PieceKind>().Where(k => k != PieceKind.King))
        {
            counts[(int)kind] += board.HandCount(Side.Sente, kind);
        }
        return counts;
    }
}
