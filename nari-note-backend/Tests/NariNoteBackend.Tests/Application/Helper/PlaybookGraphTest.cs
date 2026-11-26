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

    PlaybookBoardTransition TransitionOf(string from, string usi) =>
        this.builder.Build().Transitions.Single(t => t.FromBoard.Sfen == from && t.Usi == usi);

    [Fact]
    public void 指した局面はすべて局面になり1手ごとに遷移でつながる()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");

        var playbook = this.builder.Build();
        Assert.Equal(4, playbook.Boards.Count);
        Assert.Equal(3, playbook.Transitions.Count);
        var first = TransitionOf(this.Root, "7g7f");
        Assert.Equal(PlaybookBuilder.Play(this.Root, "7g7f"), first.ToBoard.Sfen);
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
    public void 手順前後で同じ局面に着くと1つの局面に合流する()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");

        var merged = PlaybookBuilder.Play(this.Root, "7g7f", "3c3d", "2g2f");
        var playbook = this.builder.Build();
        Assert.Single(playbook.Boards, b => b.Sfen == merged);
        Assert.Equal(2, playbook.Transitions.Count(t => t.ToBoard.Sfen == merged));
    }

    [Fact]
    public void 遷移を外すとその先にしかない局面も消える()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f", "8c8d");
        this.builder.Line("7g7f", "3c3d", "6g6f");
        var branch = PlaybookBuilder.Play(this.Root, "7g7f", "3c3d");
        var removed = PlaybookBuilder.Play(branch, "2g2f");
        this.builder.Name(removed, "飛車先");

        this.Graph().RemoveTransition(TransitionOf(branch, "2g2f"));

        var playbook = this.builder.Build();
        Assert.DoesNotContain(playbook.Boards, b => b.Sfen == removed || b.Sfen == PlaybookBuilder.Play(removed, "8c8d"));
        Assert.Equal(3, playbook.Transitions.Count);
        Assert.Empty(playbook.Nodes);
    }

    [Fact]
    public void 合流した局面は別の手順から辿れる限り残る()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");
        var merged = PlaybookBuilder.Play(this.Root, "7g7f", "3c3d", "2g2f");

        this.Graph().RemoveTransition(TransitionOf(this.Root, "7g7f"));

        var playbook = this.builder.Build();
        Assert.Contains(playbook.Boards, b => b.Sfen == merged);
        Assert.DoesNotContain(playbook.Boards, b => b.Sfen == PlaybookBuilder.Play(this.Root, "7g7f"));
        Assert.Single(playbook.Transitions, t => t.ToBoard.Sfen == merged);
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
    public void 名前もメモも無く局面が1つだけのノードは外す()
    {
        var sfen = this.builder.Line("7g7f");
        var board = this.builder.Name(sfen, "");
        var graph = this.Graph();

        graph.ReleaseNodeIfEmpty(board.Node!);

        Assert.Null(board.Node);
        Assert.Empty(this.builder.Build().Nodes);
    }

    [Fact]
    public void 名前のあるノードは外さない()
    {
        var sfen = this.builder.Line("7g7f");
        var board = this.builder.Name(sfen, "角道を開けた形");

        this.Graph().ReleaseNodeIfEmpty(board.Node!);

        Assert.NotNull(board.Node);
    }

    [Fact]
    public void 局面が無ければ例外になる()
    {
        Assert.Throws<KeyNotFoundException>(() => this.Graph().FindForceBoard(ShogiBoard.Normalize(PlaybookBuilder.Play(this.Root, "2g2f"))));
    }
}
