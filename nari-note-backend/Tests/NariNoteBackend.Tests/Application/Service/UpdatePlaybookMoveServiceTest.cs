using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UpdatePlaybookMoveServiceTest
{
    [Fact]
    public async Task 手の一行メモを保存する()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        builder.Line("7g7f");
        var playbook = builder.Build();
        repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);

        await new UpdatePlaybookMoveService(repository, new TestTimeProvider()).ExecuteAsync(
            owner.Id,
            new UpdatePlaybookMoveRequest { Id = playbook.Id, MoveId = playbook.Moves[0].Id, Memo = "角道を開ける" }
        );

        Assert.Equal("角道を開ける", playbook.Moves[0].Memo);
        await repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task 条件は同じ辺にあたる指し手すべてにそろえる()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        var a = builder.Line("7g7f", "3c3d");
        var b = builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        var c = builder.Line("1g1f", "1c1d", "7g7f", "3c3d");
        builder.Merge(a, b);
        builder.Merge(a, c);
        var fromA = builder.Play(a, "2g2f");
        var fromB = builder.Play(b, "2g2f");
        builder.Merge(fromA, fromB);
        builder.Play(c, "2g2f");
        var playbook = builder.Build();
        repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);
        var moves = playbook.Moves.Where(m => m.Usi == "2g2f").ToList();

        await new UpdatePlaybookMoveService(repository, new TestTimeProvider()).ExecuteAsync(
            owner.Id,
            new UpdatePlaybookMoveRequest { Id = playbook.Id, MoveId = moves[0].Id, Condition = "1筋を突いていない" }
        );

        Assert.Equal(["1筋を突いていない", "1筋を突いていない", ""], moves.Select(m => m.Condition));
        Assert.Equal("", moves[0].Memo);
    }
}
