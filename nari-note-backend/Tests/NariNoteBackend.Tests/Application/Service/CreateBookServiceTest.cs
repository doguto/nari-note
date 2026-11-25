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

public class CreateBookServiceTest
{
    readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly CreateBookService service;

    public CreateBookServiceTest()
    {
        this.service = new CreateBookService(this.bookRepository);
    }

    [Fact]
    public async Task 開始局面を省略すると平手から始まる()
    {
        await this.service.ExecuteAsync(this.owner.Id, new CreateBookRequest { Name = "角換わり" });

        await this.bookRepository.Received(1).CreateAsync(Arg.Is<Book>(b =>
            b.UserId == this.owner.Id
            && b.Name == "角換わり"
            && b.RootSfen == ShogiPosition.Normalize(ShogiPosition.Hirate)
            && b.Positions.Count == 1
            && b.Positions[0].Sfen == b.RootSfen
            && b.Positions[0].CandidatesScanned
            && b.ShapeGroups.Count == 1
        ));
    }

    [Fact]
    public async Task 開始局面のSFENは手数をそろえて保存する()
    {
        const string sfen = "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/2P1SPP2/PPSPP1N1P/2G4R1/LN2KG2L b Bb 27";

        await this.service.ExecuteAsync(this.owner.Id, new CreateBookRequest { Name = "腰掛け銀", RootSfen = sfen });

        await this.bookRepository.Received(1).CreateAsync(Arg.Is<Book>(b => b.RootSfen == ShogiPosition.Normalize(sfen)));
    }

    [Fact]
    public async Task 手番でない側に王手がかかっている局面では作成しない()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new CreateBookRequest { Name = "王手放置", RootSfen = "4k4/4P4/9/9/9/9/9/9/K8 b - 1" })
        );
    }

    [Fact]
    public async Task 形式の正しくないSFENでは作成しない()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new CreateBookRequest { Name = "壊れた局面", RootSfen = "abc" })
        );
        await this.bookRepository.DidNotReceiveWithAnyArgs().CreateAsync(null!);
    }
}
