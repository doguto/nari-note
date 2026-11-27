using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Helper;

public class PlaybookGraphTest
{
    readonly IPlaybookRepository repository = Substitute.For<IPlaybookRepository>();
    readonly PlaybookBuilder builder = new(new UserBuilder().Build());

    string Root => this.builder.Build().RootSfen;

    PlaybookGraph Graph() => new(this.builder.Build(), this.repository);

    PlaybookBoard BoardAt(params string[] usis) => this.builder.BoardAt(PlaybookBuilder.Play(this.Root, usis))!;

    PlaybookBoardTransition TransitionOf(string from, string usi) =>
        this.builder.Build().Transitions.Single(t => t.FromBoard.Sfen == from && t.Usi == usi);

    [Fact]
    public void 指した局面はすべて局面になり1手ごとに遷移でつながり開始局面だけがノードになる()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");

        var playbook = this.builder.Build();
        Assert.Equal(4, playbook.Boards.Count);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], playbook.Transitions.Select(t => t.Usi));
        var node = Assert.Single(playbook.Nodes);
        Assert.Equal(this.Root, Assert.Single(node.Boards).Sfen);
    }

    [Fact]
    public void 同じ手順をもう一度足しても増えない()
    {
        this.builder.Line("7g7f", "3c3d");
        this.builder.Line("7g7f", "3c3d", "2g2f");

        var playbook = this.builder.Build();
        Assert.Equal(4, playbook.Boards.Count);
        Assert.Equal(3, playbook.Transitions.Count);
    }

    [Fact]
    public void 分岐点はノードになる()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("7g7f", "3c3d", "6g6f");

        Assert.NotNull(this.BoardAt("7g7f", "3c3d").Node);
        Assert.Null(this.BoardAt("7g7f").Node);
        Assert.Equal(2, this.builder.Build().Nodes.Count);
    }

    [Fact]
    public void 手順前後で同じ局面に着くと1つの局面に合流しノードになる()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");

        var merged = this.BoardAt("7g7f", "3c3d", "2g2f");
        Assert.Single(this.builder.Build().Boards, b => b.Sfen == merged.Sfen);
        Assert.Equal(2, this.builder.Build().Transitions.Count(t => t.ToBoard == merged));
        Assert.NotNull(merged.Node);
    }

    [Fact]
    public void 辺の途中の局面から遡るとノードから出る1本目の遷移に着く()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        var last = TransitionOf(PlaybookBuilder.Play(this.Root, "7g7f", "3c3d"), "2g2f");

        var first = this.Graph().FirstOfEdge(last);

        Assert.Equal(TransitionOf(this.Root, "7g7f"), first);
    }

    [Fact]
    public void 辺を外すとその先にしかない局面とノードも消え分岐点のノードは残る()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f", "8c8d");
        this.builder.Line("7g7f", "3c3d", "6g6f");
        var branch = this.BoardAt("7g7f", "3c3d");
        var removed = PlaybookBuilder.Play(branch.Sfen, "2g2f");
        this.builder.Name(removed, "飛車先");

        this.Graph().RemoveEdge(TransitionOf(branch.Sfen, "2g2f"));

        var playbook = this.builder.Build();
        Assert.DoesNotContain(playbook.Boards, b => b.Sfen == removed || b.Sfen == PlaybookBuilder.Play(removed, "8c8d"));
        Assert.DoesNotContain(playbook.Nodes, n => n.Name == "飛車先");
        Assert.NotNull(branch.Node);
        Assert.Equal(3, playbook.Transitions.Count);
    }

    [Fact]
    public void 合流した局面は別の手順から辿れる限り残る()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");
        var merged = PlaybookBuilder.Play(this.Root, "7g7f", "3c3d", "2g2f");

        this.Graph().RemoveEdge(TransitionOf(this.Root, "7g7f"));

        var playbook = this.builder.Build();
        Assert.Contains(playbook.Boards, b => b.Sfen == merged);
        Assert.DoesNotContain(playbook.Boards, b => b.Sfen == PlaybookBuilder.Play(this.Root, "7g7f"));
        Assert.Single(playbook.Transitions, t => t.ToBoard.Sfen == merged);
    }

    [Fact]
    public void ノードに入る辺は出発のノードまで遡って丸ごと外す()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f", "8c8d");
        this.builder.Line("2g2f", "8c8d");
        var x = this.builder.Name(PlaybookBuilder.Play(this.Root, "7g7f", "3c3d"), "角道を開け合った形");

        this.Graph().RemoveEdgesInto(x.Node!);

        var playbook = this.builder.Build();
        Assert.Equal(3, playbook.Boards.Count);
        Assert.Equal(["2g2f", "8c8d"], playbook.Transitions.Select(t => t.Usi));
        Assert.DoesNotContain(playbook.Nodes, n => n.Name == "角道を開け合った形");
    }

    [Fact]
    public void ノードを解除すると局面はノードでなくなり出る遷移の条件も消える()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        var board = this.builder.Name(PlaybookBuilder.Play(this.Root, "7g7f"), "角道を開けた形");
        TransitionOf(board.Sfen, "3c3d").Condition = "後手が角道を開けたら";

        this.Graph().Unlink(board.Node!);

        Assert.Null(board.Node);
        Assert.Equal("", TransitionOf(board.Sfen, "3c3d").Condition);
        Assert.Single(this.builder.Build().Nodes);
    }

    [Fact]
    public void 分岐点と開始局面のノードは解除できない()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("7g7f", "3c3d", "6g6f");
        var graph = this.Graph();

        Assert.Throws<InvalidOperationException>(() => graph.Unlink(this.BoardAt("7g7f", "3c3d").Node!));
        Assert.Throws<InvalidOperationException>(() => graph.Unlink(this.builder.Root.Node!));
    }

    [Fact]
    public void 指せない手を含む手順は何も足さない()
    {
        Assert.Throws<ArgumentException>(() => this.Graph().AddLine(["7g7f", "7g7e"]));

        var playbook = this.builder.Build();
        Assert.Single(playbook.Boards);
        Assert.Empty(playbook.Transitions);
    }

    [Fact]
    public void 局面が無ければ例外になる()
    {
        Assert.Throws<KeyNotFoundException>(() => this.Graph().FindForceBoard(ShogiBoard.Normalize(PlaybookBuilder.Play(this.Root, "2g2f"))));
    }
}
