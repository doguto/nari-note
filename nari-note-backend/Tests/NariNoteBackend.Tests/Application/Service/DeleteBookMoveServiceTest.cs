using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class DeleteBookMoveServiceTest
{
    readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly BookBuilder builder;
    readonly Book book;
    readonly DeleteBookMoveService service;

    public DeleteBookMoveServiceTest()
    {
        this.builder = new BookBuilder(this.owner);
        this.book = this.builder.Build();
        this.bookRepository.FindForceGraphByIdAsync(this.book.Id).Returns(this.book);
        this.service = new DeleteBookMoveService(this.bookRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task 外した手の先にしかない局面と手と候補は一緒に消える()
    {
        var a = this.builder.Line("7g7f");
        var b = this.builder.Play(a, "3c3d");
        var c = this.builder.Play(b, "2g2f");
        var kept = this.builder.Line("2g2f");
        var candidate = this.builder.Candidate(kept, c);
        var first = this.book.Moves.First(m => m.ParentPositionId == this.builder.Root.Id && m.Usi == "7g7f");

        await this.service.ExecuteAsync(this.owner.Id, new DeleteBookMoveRequest { Id = this.book.Id, MoveId = first.Id });

        this.bookRepository.Received(1).RemoveMove(first);
        this.bookRepository.Received(1).RemoveMove(Arg.Is<BookMove>(m => m.Usi == "3c3d"));
        this.bookRepository.Received(1).RemoveMove(Arg.Is<BookMove>(m => m.Usi == "2g2f" && m.ParentPositionId == b.Id));
        this.bookRepository.Received(1).RemovePosition(a);
        this.bookRepository.Received(1).RemovePosition(b);
        this.bookRepository.Received(1).RemovePosition(c);
        this.bookRepository.DidNotReceive().RemovePosition(kept);
        this.bookRepository.Received(1).RemoveShapeCandidate(candidate);
        this.bookRepository.Received(3).RemoveShapeGroup(Arg.Any<ShapeGroup>());
        await this.bookRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task 別の手順からも辿れる局面は残る()
    {
        var joined = this.builder.Line("7g7f", "3c3d", "2g2f");
        this.builder.Line("2g2f", "3c3d", "7g7f");
        var first = this.book.Moves.First(m => m.ParentPositionId == this.builder.Root.Id && m.Usi == "7g7f");

        await this.service.ExecuteAsync(this.owner.Id, new DeleteBookMoveRequest { Id = this.book.Id, MoveId = first.Id });

        this.bookRepository.DidNotReceive().RemovePosition(joined);
    }

    [Fact]
    public async Task 持ち主以外は手を外せない()
    {
        this.builder.Line("7g7f");
        var move = this.book.Moves[0];

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            this.service.ExecuteAsync(new UserBuilder().Build().Id, new DeleteBookMoveRequest { Id = this.book.Id, MoveId = move.Id })
        );
    }
}
