using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class ShogiNotationTest
{
    [Fact]
    public void 手順を記譜にすると直前と同じマスは同になる()
    {
        var labels = ShogiNotation.SequenceLabels(ShogiPosition.Hirate, ["7g7f", "3c3d", "8h2b+", "3a2b"]);

        Assert.Equal(["▲７六歩", "△３四歩", "▲２二角成", "△同　銀"], labels);
    }

    [Fact]
    public void 同じマスに行ける駒が複数あるときは移動元を添える()
    {
        var position = ShogiPosition.FromSfen(ShogiPosition.Hirate);

        Assert.Equal("▲５八金(69)", ShogiNotation.MoveLabel(position, ShogiMove.Parse("6i5h")));
    }

    [Fact]
    public void 成れるのに成らない手は不成を付ける()
    {
        var position = ShogiPosition.FromSfen("4k4/9/9/2S6/9/9/9/9/4K4 b - 1");

        Assert.Equal("▲７三銀不成", ShogiNotation.MoveLabel(position, ShogiMove.Parse("7d7c")));
        Assert.Equal("▲７三銀成", ShogiNotation.MoveLabel(position, ShogiMove.Parse("7d7c+")));
    }

    [Fact]
    public void 盤上の駒も行けるマスへの駒打ちは打を付ける()
    {
        var position = ShogiPosition.FromSfen("4k4/9/9/9/9/9/9/4G4/4K4 b G 1");

        Assert.Equal("▲５七金打", ShogiNotation.MoveLabel(position, ShogiMove.Parse("G*5g")));
        Assert.Equal("▲１一金", ShogiNotation.MoveLabel(position, ShogiMove.Parse("G*1a")));
    }

    [Fact]
    public void 指せない手が来たらそこで記譜を打ち切る()
    {
        var labels = ShogiNotation.SequenceLabels(ShogiPosition.Hirate, ["7g7f", "7f7e", "3c3d"]);

        Assert.Equal(["▲７六歩"], labels);
    }
}
