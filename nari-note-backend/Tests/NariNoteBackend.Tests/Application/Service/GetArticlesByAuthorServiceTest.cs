using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticlesByAuthorServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly UserId authorId = UserId.From(Guid.CreateVersion7());
    readonly GetArticlesByAuthorService service;

    public GetArticlesByAuthorServiceTest()
    {
        this.service = new GetArticlesByAuthorService(this.articleRepository);
    }

    [Fact]
    public async Task 作者の記事を返す()
    {
        var summary = new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = "公開記事",
            AuthorId = this.authorId,
            AuthorName = "author",
            TagNames = ["振り飛車"],
            LikeCount = 1,
            PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        this.articleRepository.FindPublishedSummariesByAuthorAsync(this.authorId, 20, 0)
            .Returns((new List<ArticleSummary> { summary }, 1));

        var response = await this.service.ExecuteAsync(new GetArticlesByAuthorRequest { AuthorId = this.authorId });

        Assert.Equal(this.authorId, response.AuthorId);
        Assert.Equal("author", response.AuthorName);
        var dto = Assert.Single(response.Articles);
        Assert.Equal("公開記事", dto.Title);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task 指定した件数と位置でリポジトリから取得する()
    {
        this.articleRepository.FindPublishedSummariesByAuthorAsync(this.authorId, 10, 30)
            .Returns((new List<ArticleSummary>(), 45));

        var response = await this.service.ExecuteAsync(
            new GetArticlesByAuthorRequest { AuthorId = this.authorId, Limit = 10, Offset = 30 }
        );

        Assert.Equal("", response.AuthorName);
        Assert.Empty(response.Articles);
        Assert.Equal(45, response.TotalCount);
    }
}
