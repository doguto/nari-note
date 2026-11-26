using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class DeletePlaybookMoveServiceTest
{
    readonly IPlaybookRepository playbookRepository = Substitute.For<IPlaybookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly PlaybookBuilder builder;
    readonly Playbook playbook;
    readonly DeletePlaybookMoveService service;

    public DeletePlaybookMoveServiceTest()
    {
        this.builder = new PlaybookBuilder(this.owner);
        this.playbook = this.builder.Build();
        this.playbookRepository.FindForceGraphByIdAsync(this.playbook.Id).Returns(this.playbook);
        this.service = new DeletePlaybookMoveService(this.playbookRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task 外した手の先にしかない局面と手と候補は一緒に消える()
    {
        var a = this.builder.Line("7g7f");
        var b = this.builder.Play(a, "3c3d");
        var c = this.builder.Play(b, "2g2f");
        var kept = this.builder.Line("2g2f");
        var candidate = this.builder.Candidate(kept, c);
        var first = this.playbook.Moves.First(m => m.ParentPositionId == this.builder.Root.Id && m.Usi == "7g7f");

        await this.service.ExecuteAsync(this.owner.Id, new DeletePlaybookMoveRequest { Id = this.playbook.Id, MoveId = first.Id });

        this.playbookRepository.Received(1).RemoveMove(first);
        this.playbookRepository.Received(1).RemoveMove(Arg.Is<PlaybookMove>(m => m.Usi == "3c3d"));
        this.playbookRepository.Received(1).RemoveMove(Arg.Is<PlaybookMove>(m => m.Usi == "2g2f" && m.ParentPositionId == b.Id));
        this.playbookRepository.Received(1).RemovePosition(a);
        this.playbookRepository.Received(1).RemovePosition(b);
        this.playbookRepository.Received(1).RemovePosition(c);
        this.playbookRepository.DidNotReceive().RemovePosition(kept);
        this.playbookRepository.Received(1).RemoveShapeCandidate(candidate);
        this.playbookRepository.Received(3).RemoveShapeGroup(Arg.Any<ShapeGroup>());
        await this.playbookRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task 別の手順からも辿れる局面は残る()
    {
        var joined = this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");
        var first = this.playbook.Moves.First(m => m.ParentPositionId == this.builder.Root.Id && m.Usi == "7g7f");

        await this.service.ExecuteAsync(this.owner.Id, new DeletePlaybookMoveRequest { Id = this.playbook.Id, MoveId = first.Id });

        this.playbookRepository.DidNotReceive().RemovePosition(joined);
    }

    [Fact]
    public async Task 持ち主以外は手を外せない()
    {
        this.builder.Line("7g7f");
        var move = this.playbook.Moves[0];

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            this.service.ExecuteAsync(new UserBuilder().Build().Id, new DeletePlaybookMoveRequest { Id = this.playbook.Id, MoveId = move.Id })
        );
    }
}
