using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class SavePlaybookLinesServiceTest
{
    readonly IPlaybookRepository playbookRepository = Substitute.For<IPlaybookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly PlaybookBuilder builder;
    readonly Playbook playbook;
    readonly SavePlaybookLinesService service;

    public SavePlaybookLinesServiceTest()
    {
        this.builder = new PlaybookBuilder(this.owner);
        this.playbook = this.builder.Build();
        this.playbookRepository.FindForceGraphByIdAsync(this.playbook.Id).Returns(this.playbook);
        this.service = new SavePlaybookLinesService(this.playbookRepository, new TestTimeProvider());
    }

    SavePlaybookLinesRequest Request(params (PlaybookPosition From, string[] Usis)[] lines) => new()
    {
        Id = this.playbook.Id,
        Lines = lines.Select(l => new PlaybookLineDto { FromPositionId = l.From.Id, Usis = l.Usis.ToList() }).ToList(),
    };

    [Fact]
    public async Task 手順をまとめて局面と形グループと手にして一度だけ保存する()
    {
        await this.service.ExecuteAsync(this.owner.Id, this.Request((this.builder.Root, ["7g7f", "3c3d"])));

        this.playbookRepository.Received(2).AddPosition(Arg.Any<PlaybookPosition>());
        this.playbookRepository.Received(2).AddShapeGroup(Arg.Any<ShapeGroup>());
        this.playbookRepository.Received(1).AddMove(Arg.Is<PlaybookMove>(m => m.Usi == "7g7f" && m.Label == "▲７六歩"));
        this.playbookRepository.Received(1).AddMove(Arg.Is<PlaybookMove>(m => m.Usi == "3c3d" && m.Label == "△３四歩"));
        await this.playbookRepository.Received(1).SaveAsync();
        Assert.Equal(TestTimeProvider.DefaultUtcNow, this.playbook.UpdatedAt);
    }

    [Fact]
    public async Task 同じ保存の中で手順前後になった局面は一つにまとめる()
    {
        var response = await this.service.ExecuteAsync(
            this.owner.Id,
            this.Request((this.builder.Root, ["7g7f", "3c3d", "2g2f"]), (this.builder.Root, ["2g2f", "3c3d", "7g7f"]))
        );

        // 2 本目の最後の手は 1 本目の終点に合流するので、局面は 5 つで済む
        this.playbookRepository.Received(5).AddPosition(Arg.Any<PlaybookPosition>());
        this.playbookRepository.Received(6).AddMove(Arg.Any<PlaybookMove>());
        Assert.Equal(5, response.Positions.Count);
    }

    [Fact]
    public async Task 登録済みの手はたどるだけで保存しない()
    {
        this.builder.Line("7g7f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((this.builder.Root, ["7g7f", "3c3d"])));

        this.playbookRepository.DidNotReceive().AddMove(Arg.Any<PlaybookMove>());
        await this.playbookRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task 既存の局面へ手順前後で合流する手は既存の局面を使う()
    {
        var joined = this.builder.Line("7g7f", "3c3d", "2g2f");
        var other = this.builder.Line("2g2f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((other, ["7g7f"])));

        this.playbookRepository.DidNotReceive().AddPosition(Arg.Any<PlaybookPosition>());
        this.playbookRepository.Received(1).AddMove(Arg.Is<PlaybookMove>(m => m.ChildPosition == joined));
    }

    [Fact]
    public async Task 成る手は成った局面を作る()
    {
        var before = this.builder.Line("7g7f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((before, ["8h2b+"])));

        this.playbookRepository.Received(1).AddMove(Arg.Is<PlaybookMove>(m => m.Label == "▲２二角成"));
        this.playbookRepository.Received(1).AddPosition(Arg.Is<PlaybookPosition>(p => p.Sfen == ShogiPosition.Normalize(
            "lnsgkgsnl/1r5+B1/pppppp1pp/6p2/9/2P6/PP1PPPPPP/7R1/LNSGKGSNL w B 1"
        )));
    }

    [Theory]
    [InlineData("7g7e")]
    [InlineData("P*5e")]
    [InlineData("xx")]
    public async Task 指せない手が混ざっていたら何も保存しない(string usi)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(this.owner.Id, this.Request((this.builder.Root, ["7g7f", usi]))));
        await this.playbookRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task 持ち主以外は保存できない()
    {
        var other = new UserBuilder().Build();

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(other.Id, this.Request((this.builder.Root, ["7g7f"]))));
    }
}
