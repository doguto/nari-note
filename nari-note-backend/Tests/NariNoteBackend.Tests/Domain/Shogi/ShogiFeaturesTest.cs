using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class ShogiFeaturesTest
{
    const string Koshikake = "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/2P1SPP2/PPSPP1N1P/2G4R1/LN2KG2L b Bb 27";
    const string KoshikakeNineFile = "ln5nl/1r2gkg2/2psp1spp/p2p1pp2/1p5P1/P1P1SPP2/1PSPP1N1P/2G4R1/LN2KG2L b Bb 31";
    const string KoshikakeSenteNine = "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/P1P1SPP2/1PSPP1N1P/2G4R1/LN2KG2L b Bb 29";

    static ShogiPosition Play(params string[] moves) =>
        moves.Aggregate(ShogiPosition.FromSfen(ShogiPosition.Hirate), (p, m) => p.Apply(ShogiMove.Parse(m)));

    // 先手は四間飛車の本美濃、後手は居飛車穴熊
    static readonly string[] ShikenbishaVsAnaguma =
    [
        "7g7f", "3c3d", "6g6f", "2b3c", "2h6h", "1a1b", "5i4h", "5a4b",
        "4h3h", "4b3b", "3h2h", "3b2b", "3i3h", "2b1a", "6i5h", "3a2b",
    ];

    [Fact]
    public void 平手の初期局面は平手だけを候補にする()
    {
        Assert.Equal(["平手"], ShogiFeatures.NameSuggestions(ShogiPosition.FromSfen(ShogiPosition.Hirate)));
    }

    [Fact]
    public void 角換わり腰掛け銀を見分ける()
    {
        var suggestions = ShogiFeatures.NameSuggestions(ShogiPosition.FromSfen(Koshikake));

        Assert.Equal(["角換わり腰掛け銀", "角換わり"], suggestions);
    }

    [Fact]
    public void 端歩の突き合いを名前の候補に添える()
    {
        var suggestions = ShogiFeatures.NameSuggestions(ShogiPosition.FromSfen(KoshikakeNineFile));

        Assert.Equal("角換わり腰掛け銀 9筋突き合い", suggestions[0]);
    }

    [Fact]
    public void 振り飛車の戦型と後手の囲いを盤を回して見分ける()
    {
        var position = Play(ShikenbishaVsAnaguma);

        Assert.Equal("四間飛車", ShogiFeatures.Style(position, Side.Sente));
        Assert.Equal("居飛車", ShogiFeatures.Style(position, Side.Gote));
        Assert.Equal("本美濃", ShogiFeatures.Castle(position, Side.Sente));
        Assert.Equal("居飛車穴熊", ShogiFeatures.Castle(position, Side.Gote));
        Assert.Contains("先手四間飛車", ShogiFeatures.NameSuggestions(position));
        Assert.Contains("本美濃 対 居飛車穴熊", ShogiFeatures.NameSuggestions(position));
    }

    [Fact]
    public void 本美濃から金を上がると高美濃になる()
    {
        var position = Play([.. ShikenbishaVsAnaguma, "4g4f", "9c9d", "5h4g"]);

        Assert.Equal("高美濃", ShogiFeatures.Castle(position, Side.Sente));
    }

    [Fact]
    public void 端歩だけの違いは端歩の状態を条件の候補にする()
    {
        var suggestions = ShogiFeatures.ConditionSuggestions(
            ShogiPosition.FromSfen(KoshikakeNineFile),
            [ShogiPosition.FromSfen(Koshikake)]
        );

        Assert.Equal("9筋を突き合っている", suggestions[0]);
        Assert.Contains("△歩が９四、▲歩が９六", suggestions);
    }

    [Fact]
    public void 片方だけ突いた端歩は突いた側を条件にする()
    {
        var suggestions = ShogiFeatures.ConditionSuggestions(
            ShogiPosition.FromSfen(KoshikakeSenteNine),
            [ShogiPosition.FromSfen(Koshikake), ShogiPosition.FromSfen(KoshikakeNineFile)]
        );

        Assert.Equal("9筋を先手だけ突いている", suggestions[0]);
    }

    [Fact]
    public void 同じ局面や離れすぎた局面からは条件を作らない()
    {
        var suggestions = ShogiFeatures.ConditionSuggestions(
            ShogiPosition.FromSfen(Koshikake),
            [ShogiPosition.FromSfen(Koshikake), ShogiPosition.FromSfen(ShogiPosition.Hirate)]
        );

        Assert.Empty(suggestions);
    }
}
