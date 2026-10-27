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
        this.articleRepository.SearchSummariesAsync("四間飛車", 11, 20).Returns(new List<ArticleSummary> { summary });

        var response = await this.service.ExecuteAsync(
            new SearchArticlesRequest { Keyword = "四間飛車", Limit = 10, Offset = 20 }
        );

        var dto = Assert.Single(response.Articles);
        Assert.Equal(summary.Id, dto.Id);
        Assert.Equal("四間飛車の基本", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.False(response.HasNext);
    }

    [Fact]
    public async Task 指定件数より多く見つかった場合は指定件数だけ返し次のページがあると判定する()
    {
        var summaries = Enumerable.Range(1, 3).Select(i => Summary($"記事{i}")).ToList();
        this.articleRepository.SearchSummariesAsync("記事", 3, 0).Returns(summaries);

        var response = await this.service.ExecuteAsync(
            new SearchArticlesRequest { Keyword = "記事", Limit = 2, Offset = 0 }
        );

        Assert.Equal(["記事1", "記事2"], response.Articles.Select(a => a.Title));
        Assert.True(response.HasNext);
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
