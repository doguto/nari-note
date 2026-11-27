using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetPlaybookServiceTest
{
    readonly IPlaybookRepository repository = Substitute.For<IPlaybookRepository>();

    [Fact]
    public async Task ノードと局面と遷移を返す()
    {
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        builder.Line("7g7f", "3c3d", "2g2f");
        builder.Line("7g7f", "3c3d", "6g6f");
        builder.Name(PlaybookBuilder.Play(builder.Build().RootSfen, "7g7f", "3c3d"), "角道を開け合った形");
        var playbook = builder.Build();
        this.repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);

        var response = await new GetPlaybookService(this.repository).ExecuteAsync(owner.Id, new GetPlaybookRequest { Id = playbook.Id });

        Assert.Equal(playbook.RootSfen, response.RootSfen);
        Assert.Equal(5, response.Boards.Count);
        Assert.Equal(4, response.Transitions.Count);
        Assert.Equal(2, response.Nodes.Count);
        var node = Assert.Single(response.Nodes, n => n.Name == "角道を開け合った形");
        var board = Assert.Single(response.Boards, b => b.NodeId == node.Id);
        Assert.Equal(PlaybookBuilder.Play(playbook.RootSfen, "7g7f", "3c3d"), board.Sfen);
        Assert.Null(response.Boards.Single(b => b.Id == builder.BoardAt(PlaybookBuilder.Play(playbook.RootSfen, "7g7f"))!.Id).Sfen);
        var first = Assert.Single(response.Transitions, t => t.FromBoardId == builder.Root.Id);
        Assert.Equal("7g7f", first.Usi);
    }

    [Fact]
    public async Task 他人の定跡は取得できない()
    {
        var playbook = new PlaybookBuilder(new UserBuilder().Build()).Build();
        this.repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            new GetPlaybookService(this.repository).ExecuteAsync(new UserBuilder().Build().Id, new GetPlaybookRequest { Id = playbook.Id })
        );
    }
}
