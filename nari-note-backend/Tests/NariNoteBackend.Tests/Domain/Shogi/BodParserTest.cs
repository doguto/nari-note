using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class BodParserTest
{
    const string Bod = """
                       後手の持駒：歩二　銀
                         ９ ８ ７ ６ ５ ４ ３ ２ １
                       +---------------------------+
                       | ・ ・ ・ ・ ・ ・ ・v桂v香|一
                       | ・ ・ ・ ・ ・ ・v金v玉 ・|二
                       | ・ ・ ・ ・ ・ ・v歩v歩 ・|三
                       | ・ ・ ・ ・ ・ ・ ・ ・ ・|四
                       | ・ ・ ・ ・ ・ ・ ・ ・ ・|五
                       | ・ ・ ・ ・ ・ ・ ・ ・ ・|六
                       | ・ ・ ・ ・ ・ ・ ・ ・ ・|七
                       | ・ ・ ・ ・ ・ ・ ・ と ・|八
                       |v成銀 ・ ・ ・ ・ ・ ・ 龍 ・|九
                       +---------------------------+
                       先手の持駒：金十一　成銀
                       """;

    [Fact]
    public void 盤上の駒を先手後手と成駒を区別して読む()
    {
        var position = BodParser.Parse(Bod);

        Assert.Equal(new ShogiPiece("香", PieceOwner.Gote), position.Board[0, 8]);
        Assert.Equal(new ShogiPiece("玉", PieceOwner.Gote), position.Board[1, 7]);
        Assert.Equal(new ShogiPiece("と", PieceOwner.Sente), position.Board[7, 7]);
        Assert.Equal(new ShogiPiece("龍", PieceOwner.Sente), position.Board[8, 7]);
        Assert.Equal(new ShogiPiece("成銀", PieceOwner.Gote), position.Board[8, 0]);
        Assert.Null(position.Board[4, 4]);
        Assert.Equal(9, position.Board.Cast<ShogiPiece?>().Count(p => p != null));
    }

    [Fact]
    public void 持駒を漢数字の枚数付きで読む()
    {
        var position = BodParser.Parse(Bod);

        Assert.Equal([("歩", 2), ("銀", 1)], position.GoteCaptured.Select(p => (p.Type, p.Count)));
        Assert.Equal([("金", 11), ("成銀", 1)], position.SenteCaptured.Select(p => (p.Type, p.Count)));
    }

    [Fact]
    public void 盤面が無ければ駒の無い局面になる()
    {
        var position = BodParser.Parse("先手の持駒：なし");

        Assert.False(position.HasAnyPiece);
        Assert.Empty(position.SenteCaptured);
    }
}
