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

public class GetBookServiceTest
{
    [Fact]
    public async Task 開始局面のIDを返す()
    {
        var repository = Substitute.For<IBookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new BookBuilder(owner);
        builder.Line("7g7f");
        var book = builder.Build();
        repository.FindForceByIdAsync(book.Id).Returns(book);
        repository.FindPositionsAsync(book.Id).Returns(book.Positions);

        var response = await new GetBookService(repository).ExecuteAsync(owner.Id, new GetBookRequest { Id = book.Id });

        Assert.Equal(builder.Root.Id, response.RootPositionId);
    }
}
