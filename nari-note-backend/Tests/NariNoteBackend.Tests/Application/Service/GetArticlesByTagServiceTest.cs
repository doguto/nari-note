using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticlesByTagServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly GetArticlesByTagService service;

    public GetArticlesByTagServiceTest()
    {
        this.service = new GetArticlesByTagService(this.articleRepository);
    }

    [Theory]
    [InlineData("振り飛車")]
    [InlineData("Shogi_2026")]
    [InlineData("v1.0-beta")]
    public async Task タグ名に一致する記事を返す(string tagName)
    {
        var summary = new ArticleSummary
        {
            Id = ArticleId.From(Guid.CreateVersion7()),
            Title = "記事",
            AuthorId = UserId.From(Guid.CreateVersion7()),
            AuthorName = "author",
            TagNames = [tagName],
            LikeCount = 1,
            PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        this.articleRepository.FindSummariesByTagAsync(tagName, 20, 0).Returns((new List<ArticleSummary> { summary }, 1));

        var response = await this.service.ExecuteAsync(new GetArticlesByTagRequest { TagName = tagName });

        var dto = Assert.Single(response.Articles);
        Assert.Equal("記事", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal([tagName], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task 指定した件数と位置でリポジトリから取得する()
    {
        this.articleRepository.FindSummariesByTagAsync("Shogi", 10, 30).Returns((new List<ArticleSummary>(), 45));

        var response = await this.service.ExecuteAsync(new GetArticlesByTagRequest { TagName = "Shogi", Limit = 10, Offset = 30 });

        Assert.Empty(response.Articles);
        Assert.Equal(45, response.TotalCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("振り 飛車")]
    [InlineData("tag!")]
    [InlineData("100%")]
    public async Task 使用できない文字を含むタグ名は検索せずにエラーにする(string tagName)
    {
        var request = new GetArticlesByTagRequest { TagName = tagName };

        await Assert.ThrowsAsync<ValidationException>(() => this.service.ExecuteAsync(request));

        await this.articleRepository.DidNotReceive().FindSummariesByTagAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
    }
}
