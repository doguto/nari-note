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

public class SaveBookLinesServiceTest
{
    readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly BookBuilder builder;
    readonly Book book;
    readonly SaveBookLinesService service;

    public SaveBookLinesServiceTest()
    {
        this.builder = new BookBuilder(this.owner);
        this.book = this.builder.Build();
        this.bookRepository.FindForceGraphByIdAsync(this.book.Id).Returns(this.book);
        this.service = new SaveBookLinesService(this.bookRepository, new TestTimeProvider());
    }

    SaveBookLinesRequest Request(params (BookPosition From, string[] Usis)[] lines) => new()
    {
        Id = this.book.Id,
        Lines = lines.Select(l => new BookLineDto { FromPositionId = l.From.Id, Usis = l.Usis.ToList() }).ToList(),
    };

    [Fact]
    public async Task 手順をまとめて局面と形グループと手にして一度だけ保存する()
    {
        await this.service.ExecuteAsync(this.owner.Id, this.Request((this.builder.Root, ["7g7f", "3c3d"])));

        this.bookRepository.Received(2).AddPosition(Arg.Any<BookPosition>());
        this.bookRepository.Received(2).AddShapeGroup(Arg.Any<ShapeGroup>());
        this.bookRepository.Received(1).AddMove(Arg.Is<BookMove>(m => m.Usi == "7g7f" && m.Label == "▲７六歩"));
        this.bookRepository.Received(1).AddMove(Arg.Is<BookMove>(m => m.Usi == "3c3d" && m.Label == "△３四歩"));
        await this.bookRepository.Received(1).SaveAsync();
        Assert.Equal(TestTimeProvider.DefaultUtcNow, this.book.UpdatedAt);
    }

    [Fact]
    public async Task 同じ保存の中で手順前後になった局面は一つにまとめる()
    {
        var response = await this.service.ExecuteAsync(
            this.owner.Id,
            this.Request((this.builder.Root, ["7g7f", "3c3d", "2g2f"]), (this.builder.Root, ["2g2f", "3c3d", "7g7f"]))
        );

        // 2 本目の最後の手は 1 本目の終点に合流するので、局面は 5 つで済む
        this.bookRepository.Received(5).AddPosition(Arg.Any<BookPosition>());
        this.bookRepository.Received(6).AddMove(Arg.Any<BookMove>());
        Assert.Equal(5, response.Positions.Count);
    }

    [Fact]
    public async Task 登録済みの手はたどるだけで保存しない()
    {
        this.builder.Line("7g7f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((this.builder.Root, ["7g7f", "3c3d"])));

        this.bookRepository.DidNotReceive().AddMove(Arg.Any<BookMove>());
        await this.bookRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task 既存の局面へ手順前後で合流する手は既存の局面を使う()
    {
        var joined = this.builder.Line("7g7f", "3c3d", "2g2f");
        var other = this.builder.Line("2g2f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((other, ["7g7f"])));

        this.bookRepository.DidNotReceive().AddPosition(Arg.Any<BookPosition>());
        this.bookRepository.Received(1).AddMove(Arg.Is<BookMove>(m => m.ChildPosition == joined));
    }

    [Fact]
    public async Task 成る手は成った局面を作る()
    {
        var before = this.builder.Line("7g7f", "3c3d");

        await this.service.ExecuteAsync(this.owner.Id, this.Request((before, ["8h2b+"])));

        this.bookRepository.Received(1).AddMove(Arg.Is<BookMove>(m => m.Label == "▲２二角成"));
        this.bookRepository.Received(1).AddPosition(Arg.Is<BookPosition>(p => p.Sfen == ShogiPosition.Normalize(
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
        await this.bookRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task 持ち主以外は保存できない()
    {
        var other = new UserBuilder().Build();

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(other.Id, this.Request((this.builder.Root, ["7g7f"]))));
    }
}
