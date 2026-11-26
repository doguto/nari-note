using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support.Builder;

namespace NariNoteBackend.Tests.Application.Helper;

public class PlaybookTreeTest
{
    static readonly string Root = ShogiBoard.Normalize(ShogiBoard.Hirate);

    static PlaybookTreeLayout Compress(PlaybookTree tree, params string[] keep) => tree.Compress(keep.Contains);

    [Fact]
    public void 一本道の手順は開始局面から終点までの1つの手順になり終点はノードにならない()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d", "2g2f"]);

        var layout = Compress(tree);

        Assert.Equal([Root], layout.Boards);
        var step = Assert.Single(layout.Steps);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], step.Usis);
        Assert.Null(step.ToSfen);
    }

    [Fact]
    public void 手順の途中で分かれると分岐点がノードになり手順が分かれる()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d", "2g2f"]);
        tree.AddLine(["7g7f", "3c3d", "6g6f"]);

        var layout = Compress(tree);

        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        Assert.True(layout.Boards.SetEquals([Root, branch]));
        Assert.Contains(layout.Steps, s => s.FromSfen == Root && s.Usis.SequenceEqual(["7g7f", "3c3d"]) && s.ToSfen == branch);
        Assert.Contains(layout.Steps, s => s.FromSfen == branch && s.Usis.SequenceEqual(["2g2f"]) && s.ToSfen == null);
        Assert.Contains(layout.Steps, s => s.FromSfen == branch && s.Usis.SequenceEqual(["6g6f"]) && s.ToSfen == null);
    }

    [Fact]
    public void 手順前後で同じ局面に着くと合流点がノードになる()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d", "2g2f", "8c8d"]);
        tree.AddLine(["2g2f", "3c3d", "7g7f"]);

        var layout = Compress(tree);

        var merged = PlaybookBuilder.Play(Root, "7g7f", "3c3d", "2g2f");
        Assert.Contains(merged, layout.Boards);
        Assert.Equal(2, layout.Steps.Count(s => s.ToSfen == merged));
        Assert.Contains(layout.Steps, s => s.FromSfen == merged && s.Usis.SequenceEqual(["8c8d"]));
    }

    [Fact]
    public void 終点から続けて指すと同じ手順が伸びる()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d"]);
        tree.AddLine(["7g7f", "3c3d", "2g2f", "8c8d"]);

        var step = Assert.Single(Compress(tree).Steps);

        Assert.Equal(["7g7f", "3c3d", "2g2f", "8c8d"], step.Usis);
    }

    [Fact]
    public void 残すと指定した局面は分岐でなくてもノードになる()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d", "2g2f"]);
        var kept = PlaybookBuilder.Play(Root, "7g7f");

        var layout = Compress(tree, kept);

        Assert.Contains(kept, layout.Boards);
        Assert.Equal(2, layout.Steps.Count);
    }

    [Fact]
    public void 手を外すとその先にしかない局面も消える()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f", "3c3d", "2g2f"]);
        tree.AddLine(["7g7f", "3c3d", "6g6f"]);
        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");

        Assert.True(tree.RemoveMove(branch, "6g6f"));

        var step = Assert.Single(Compress(tree).Steps);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], step.Usis);
        Assert.False(tree.Contains(PlaybookBuilder.Play(branch, "6g6f")));
    }

    [Fact]
    public void 無い手は外せない()
    {
        var tree = new PlaybookTree(Root);
        tree.AddLine(["7g7f"]);

        Assert.False(tree.RemoveMove(Root, "2g2f"));
    }

    [Fact]
    public void 指せない手を含む手順は何も足さない()
    {
        var tree = new PlaybookTree(Root);

        Assert.Throws<ArgumentException>(() => tree.AddLine(["7g7f", "7g7e"]));

        Assert.Empty(Compress(tree).Steps);
    }
}
