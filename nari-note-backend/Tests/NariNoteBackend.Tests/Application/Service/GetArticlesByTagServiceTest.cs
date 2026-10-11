using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
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
        var author = new UserBuilder().WithName("author").Build();
        var liker = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("記事").WithTags(tagName).LikedBy(liker).Build();
        this.articleRepository.FindByTagAsync(tagName).Returns(new List<Article> { article });

        var response = await this.service.ExecuteAsync(new GetArticlesByTagRequest { TagName = tagName });

        var dto = Assert.Single(response.Articles);
        Assert.Equal("記事", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal([tagName], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
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

        await this.articleRepository.DidNotReceive().FindByTagAsync(Arg.Any<string>());
    }
}
