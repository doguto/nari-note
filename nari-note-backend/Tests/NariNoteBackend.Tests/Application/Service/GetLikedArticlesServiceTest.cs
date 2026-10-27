using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetLikedArticlesServiceTest
{
    readonly ILikeRepository likeRepository = Substitute.For<ILikeRepository>();
    readonly UserId userId = UserId.From(Guid.CreateVersion7());
    readonly GetLikedArticlesService service;

    public GetLikedArticlesServiceTest()
    {
        this.service = new GetLikedArticlesService(this.likeRepository);
    }

    static ArticleSummary Summary(string title, int likeCount, params string[] tagNames)
    {
        return new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = title,
            AuthorId = UserId.From(Guid.CreateVersion7()),
            AuthorName = "author",
            TagNames = tagNames.ToList(),
            LikeCount = likeCount,
            PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    [Fact]
    public async Task ユーザーがいいねした記事を返す()
    {
        this.likeRepository.FindLikedArticleSummariesByUserAsync(this.userId, 20, 0)
            .Returns((new List<ArticleSummary> { Summary("1件目", 2, "振り飛車"), Summary("2件目", 1) }, 2));

        var response = await this.service.ExecuteAsync(new GetLikedArticlesRequest { UserId = this.userId });

        Assert.Equal(this.userId, response.UserId);
        Assert.Equal(["1件目", "2件目"], response.Articles.Select(a => a.Title));
        Assert.Equal("author", response.Articles[0].AuthorName);
        Assert.Equal(["振り飛車"], response.Articles[0].Tags);
        Assert.Equal([2, 1], response.Articles.Select(a => a.LikeCount));
        Assert.Equal(2, response.TotalCount);
    }

    [Fact]
    public async Task 指定した件数と位置でリポジトリから取得する()
    {
        this.likeRepository.FindLikedArticleSummariesByUserAsync(this.userId, 10, 30)
            .Returns((new List<ArticleSummary>(), 45));

        var response = await this.service.ExecuteAsync(
            new GetLikedArticlesRequest { UserId = this.userId, Limit = 10, Offset = 30 }
        );

        Assert.Empty(response.Articles);
        Assert.Equal(45, response.TotalCount);
    }
}
