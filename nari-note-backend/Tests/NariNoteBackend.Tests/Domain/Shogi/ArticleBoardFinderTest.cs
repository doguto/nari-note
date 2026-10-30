using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support.Builder;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class ArticleBoardFinderTest
{
    const string Bod = """
                       後手の持駒：なし
                       +---------------------------+
                       | ・ ・ ・ ・v玉 ・ ・ ・ ・|一
                       +---------------------------+
                       先手の持駒：金
                       """;

    const string Kif = """
                          1 ７六歩(77)
                          2 ３四歩(33)
                       """;

    readonly User author = new UserBuilder().Build();

    [Fact]
    public void 盤面埋め込みを棋譜埋め込みより優先する()
    {
        var article = new ArticleBuilder(this.author)
                      .WithBody($"```kifu\n対局_1\n```\n\n本文\n\n```bod\n{Bod}\n```")
                      .WithKifu("対局", Kif)
                      .Build();

        var position = ArticleBoardFinder.Find(article.Body, article.Kifus);

        Assert.NotNull(position);
        Assert.Equal(new ShogiPiece("玉", PieceOwner.Gote), position.Board[0, 4]);
    }

    [Fact]
    public void 駒の無い盤面埋め込みは飛ばして棋譜埋め込みを使う()
    {
        var article = new ArticleBuilder(this.author)
                      .WithBody("```bod\n先手の持駒：なし\n```\n```kifu\n対局_1\n```")
                      .WithKifu("対局", Kif)
                      .Build();

        var position = ArticleBoardFinder.Find(article.Body, article.Kifus);

        Assert.NotNull(position);
        Assert.Equal(new ShogiPiece("歩", PieceOwner.Sente), position.Board[5, 2]);
        Assert.Null(position.Board[6, 2]);
    }

    [Fact]
    public void 手数の無い棋譜埋め込みは初期配置にする()
    {
        var article = new ArticleBuilder(this.author)
                      .WithBody("```kifu\n名前_付き_棋譜\n```")
                      .WithKifu("名前_付き_棋譜", Kif)
                      .Build();

        var position = ArticleBoardFinder.Find(article.Body, article.Kifus);

        Assert.NotNull(position);
        Assert.Equal(new ShogiPiece("歩", PieceOwner.Sente), position.Board[6, 2]);
    }

    [Fact]
    public void 埋め込みが無いか棋譜が見つからなければnullを返す()
    {
        Assert.Null(ArticleBoardFinder.Find("本文だけ", []));
        Assert.Null(ArticleBoardFinder.Find("```kifu\n無い棋譜_3\n```", []));
    }
}
