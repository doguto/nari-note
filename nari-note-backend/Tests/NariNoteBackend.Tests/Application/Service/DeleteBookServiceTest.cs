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

public class DeleteBookServiceTest
{
    [Fact]
    public async Task 持ち主だけが削除できる()
    {
        var repository = Substitute.For<IBookRepository>();
        var owner = new UserBuilder().Build();
        var book = new BookBuilder(owner).Build();
        repository.FindForceByIdAsync(book.Id).Returns(book);
        var service = new DeleteBookService(repository);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.ExecuteAsync(new UserBuilder().Build().Id, new DeleteBookRequest { Id = book.Id }));
        await service.ExecuteAsync(owner.Id, new DeleteBookRequest { Id = book.Id });

        await repository.Received(1).DeleteAsync(book.Id);
    }
}
