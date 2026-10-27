using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class SearchArticlesServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly SearchArticlesService service;

    public SearchArticlesServiceTest()
    {
        this.service = new SearchArticlesService(this.articleRepository);
    }

    [Fact]
    public async Task キーワードと件数と位置を指定して検索した記事を返す()
    {
        var summary = Summary("四間飛車の基本");
        this.articleRepository.SearchSummariesAsync("四間飛車", 10, 20).Returns((new List<ArticleSummary> { summary }, 21));

        var response = await this.service.ExecuteAsync(
            new SearchArticlesRequest { Keyword = "四間飛車", Limit = 10, Offset = 20 }
        );

        var dto = Assert.Single(response.Articles);
        Assert.Equal(summary.Id, dto.Id);
        Assert.Equal("四間飛車の基本", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.Equal(21, response.TotalCount);
    }

    static ArticleSummary Summary(string title)
    {
        return new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = title,
            AuthorId = UserId.From(Guid.CreateVersion7()),
            AuthorName = "author",
            TagNames = ["振り飛車"],
            LikeCount = 1,
            PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
