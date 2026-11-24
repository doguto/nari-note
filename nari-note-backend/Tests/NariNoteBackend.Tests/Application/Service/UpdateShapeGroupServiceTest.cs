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

public class UpdateShapeGroupServiceTest
{
    [Fact]
    public async Task 指定した項目だけを更新する()
    {
        var repository = Substitute.For<IBookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new BookBuilder(owner);
        var group = builder.Root.ShapeGroup;
        group.Name = "平手";
        var book = builder.Build();
        repository.FindForceGraphByIdAsync(book.Id).Returns(book);

        await new UpdateShapeGroupService(repository, new TestTimeProvider()).ExecuteAsync(
            owner.Id,
            new UpdateShapeGroupRequest { Id = book.Id, GroupId = group.Id, Memo = "初期局面" }
        );

        Assert.Equal("平手", group.Name);
        Assert.Equal("初期局面", group.Memo);
    }
}
