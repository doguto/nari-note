using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class ShogiDiffTest
{
    const string Sample = "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/2P1SPP2/PPSPP1N1P/2G4R1/LN2KG2L b Bb 27";
    const string NineFile = "ln5nl/1r2gkg2/2psp1spp/p2p1pp2/1p5P1/P1P1SPP2/1PSPP1N1P/2G4R1/LN2KG2L b Bb 31";

    [Fact]
    public void 端歩の突き合いの違いは2単位になる()
    {
        var units = ShogiDiff.Units(ShogiPosition.FromSfen(Sample), ShogiPosition.FromSfen(NineFile));

        Assert.Equal(["△歩 ９三→９四", "▲歩 ９七→９六"], units.Select(u => u.Label()));
    }

    [Fact]
    public void 同じ局面の差分は空になる()
    {
        Assert.Equal("", ShogiDiff.Describe(ShogiPosition.FromSfen(Sample), ShogiPosition.FromSfen(Sample)));
    }

    [Fact]
    public void 持駒になった駒は持駒への移動として数える()
    {
        var before = ShogiPosition.FromSfen(ShogiPosition.Hirate);
        var after = before.Apply(ShogiMove.Parse("7g7f"))
                          .Apply(ShogiMove.Parse("3c3d"))
                          .Apply(ShogiMove.Parse("8h2b+"));

        var labels = ShogiDiff.Units(before, after).Select(u => u.Label()).ToList();

        Assert.Contains("▲歩 ７七→７六", labels);
        Assert.Contains("△歩 ３三→３四", labels);
        Assert.Contains("▲馬 ８八→２二", labels);
        Assert.Contains("△角 ２二 → ▲角 持駒", labels);
    }
}
