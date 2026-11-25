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

public class SplitShapeGroupServiceTest
{
    readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly BookBuilder builder;
    readonly Book book;
    readonly SplitShapeGroupService service;

    public SplitShapeGroupServiceTest()
    {
        this.builder = new BookBuilder(this.owner);
        this.book = this.builder.Build();
        this.bookRepository.FindForceGraphByIdAsync(this.book.Id).Returns(this.book);
        this.service = new SplitShapeGroupService(this.bookRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task まとめた局面を新しい形に分ける()
    {
        var a = this.builder.Line("7g7f", "3c3d");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        this.builder.Merge(a, b);
        var original = a.ShapeGroup;

        await this.service.ExecuteAsync(this.owner.Id, new SplitShapeGroupRequest { Id = this.book.Id, PositionId = b.Id });

        Assert.NotSame(original, b.ShapeGroup);
        Assert.Same(original, a.ShapeGroup);
        this.bookRepository.Received(1).AddShapeGroup(b.ShapeGroup);
        await this.bookRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task 一局面だけの形は分けられない()
    {
        var a = this.builder.Line("7g7f");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new SplitShapeGroupRequest { Id = this.book.Id, PositionId = a.Id })
        );
    }
}
