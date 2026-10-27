using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticlesServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly GetArticlesService service;

    public GetArticlesServiceTest()
    {
        this.service = new GetArticlesService(this.articleRepository);
    }

    [Fact]
    public async Task 指定した件数と位置で取得した記事を返す()
    {
        var publishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var summary = new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = "記事",
            AuthorId = UserId.From(Guid.CreateVersion7()),
            AuthorName = "author",
            AuthorProfileImage = "https://images.test/author",
            TagNames = ["振り飛車"],
            LikeCount = 1,
            PublishedAt = publishedAt,
            UpdatedAt = publishedAt
        };
        this.articleRepository.FindLatestSingleSummariesAsync(10, 20).Returns(new List<ArticleSummary> { summary });

        var response = await this.service.ExecuteAsync(new GetArticlesRequest { Limit = 10, Offset = 20 });

        var dto = Assert.Single(response.Articles);
        Assert.Equal(summary.Id, dto.Id);
        Assert.Equal("記事", dto.Title);
        Assert.Equal(summary.AuthorId, dto.AuthorId);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal("https://images.test/author", dto.UserIconImageUrl);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(publishedAt, dto.PublishedAt);
        Assert.Equal(publishedAt, dto.UpdatedAt);
    }

    [Fact]
    public async Task 記事が無ければ空の一覧を返す()
    {
        this.articleRepository.FindLatestSingleSummariesAsync(20, 0).Returns(new List<ArticleSummary>());

        var response = await this.service.ExecuteAsync(new GetArticlesRequest());

        Assert.Empty(response.Articles);
    }
}
