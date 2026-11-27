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
    public async Task ノードと盤面と手順を返す()
    {
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        builder.Line("7g7f", "3c3d", "2g2f");
        builder.Line("7g7f", "3c3d", "6g6f");
        var playbook = builder.Build();
        this.repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);

        var response = await new GetPlaybookService(this.repository).ExecuteAsync(owner.Id, new GetPlaybookRequest { Id = playbook.Id });

        Assert.Equal(playbook.RootSfen, response.RootSfen);
        Assert.Equal(2, response.Boards.Count);
        Assert.Equal(2, response.Nodes.Count);
        Assert.Equal(3, response.Transitions.Count);
        var first = response.Transitions.Single(s => s.FromBoardId == builder.Root.Id);
        Assert.Equal(["7g7f", "3c3d"], first.Usis);
        Assert.NotNull(first.ToBoardId);
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
