using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetDraftArticlesServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly GetDraftArticlesService service;

    public GetDraftArticlesServiceTest()
    {
        this.service = new GetDraftArticlesService(this.articleRepository);
    }

    [Fact]
    public async Task 自分の下書きを返す()
    {
        var authorId = UserId.From(Guid.CreateVersion7());
        var updatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var draft = new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = "下書き",
            AuthorId = authorId,
            AuthorName = "author",
            TagNames = ["振り飛車"],
            UpdatedAt = updatedAt
        };
        this.articleRepository.FindDraftSummariesByAuthorAsync(authorId).Returns(new List<ArticleSummary> { draft });

        var response = await this.service.ExecuteAsync(authorId);

        var dto = Assert.Single(response.Articles);
        Assert.Equal(draft.Id, dto.Id);
        Assert.Equal("下書き", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.False(dto.IsPublished);
        Assert.Null(dto.PublishedAt);
        Assert.Equal(updatedAt, dto.UpdatedAt);
        Assert.Equal(0, dto.LikeCount);
    }
}
