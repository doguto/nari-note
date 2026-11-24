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

public class GetMyBooksServiceTest
{
    [Fact]
    public async Task 自分の定跡の一覧を返す()
    {
        var repository = Substitute.For<IBookRepository>();
        var owner = new UserBuilder().Build();
        var summary = new BookSummary
        {
            Id = NariNoteBackend.Domain.ValueObject.BookId.From(1),
            Name = "角換わり",
            PositionCount = 12,
            UpdatedAt = TestTimeProvider.DefaultUtcNow,
        };
        repository.FindSummariesByUserAsync(owner.Id).Returns([summary]);

        var response = await new GetMyBooksService(repository).ExecuteAsync(owner.Id);

        var book = Assert.Single(response.Books);
        Assert.Equal("角換わり", book.Name);
        Assert.Equal(12, book.PositionCount);
    }
}
