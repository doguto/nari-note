using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class KifParserTest
{
    const string Kif = """
                       先手：先手さん
                       後手：後手さん
                       手数----指手---------消費時間--
                          1 ７六歩(77)   ( 0:00/00:00:00)
                          2 ３四歩(33)   ( 0:00/00:00:00)
                          3 ２二角成(88) ( 0:00/00:00:00)
                       *コメント
                          4 同　銀(31)   ( 0:00/00:00:00)
                          5 ４五角打     ( 0:00/00:00:00)
                          6 投了
                          7 ５八金右(49)
                       """;

    [Fact]
    public void 指し手を投了まで読む()
    {
        var moves = KifParser.ParseMoves(Kif);

        Assert.Equal([1, 2, 3, 4, 5], moves.Select(m => m.MoveNumber));
        Assert.Equal(new KifMove(3, 2, 2, "角", 8, 8, false, true), moves[2]);
        Assert.Equal(new KifMove(4, 2, 2, "銀", 3, 1, false, false), moves[3]);
        Assert.Equal(new KifMove(5, 4, 5, "角", null, null, true, false), moves[4]);
    }

    [Fact]
    public void 手数0は平手の初期配置になる()
    {
        var position = KifParser.GetPositionAtMove(KifParser.ParseMoves(Kif), 0);

        Assert.Equal(new ShogiPiece("玉", PieceOwner.Sente), position.Board[8, 4]);
        Assert.Equal(new ShogiPiece("王", PieceOwner.Gote), position.Board[0, 4]);
        Assert.Equal(40, position.Board.Cast<ShogiPiece?>().Count(p => p != null));
    }

    [Fact]
    public void 駒を取ると成駒は元に戻して持駒に加える()
    {
        var position = KifParser.GetPositionAtMove(KifParser.ParseMoves(Kif), 4);

        Assert.Equal(new ShogiPiece("銀", PieceOwner.Gote), position.Board[1, 7]);
        Assert.Null(position.Board[0, 6]);
        Assert.Equal([("角", 1)], position.SenteCaptured.Select(p => (p.Type, p.Count)));
        Assert.Equal([("角", 1)], position.GoteCaptured.Select(p => (p.Type, p.Count)));
    }

    [Fact]
    public void 駒を打つと持駒から減らす()
    {
        var position = KifParser.GetPositionAtMove(KifParser.ParseMoves(Kif), 5);

        Assert.Equal(new ShogiPiece("角", PieceOwner.Sente), position.Board[4, 5]);
        Assert.Empty(position.SenteCaptured);
    }

    [Fact]
    public void 手数が指し手の数を超えるときは最後の局面になる()
    {
        var position = KifParser.GetPositionAtMove(KifParser.ParseMoves(Kif), 100);

        Assert.Equal(new ShogiPiece("角", PieceOwner.Sente), position.Board[4, 5]);
    }
}
