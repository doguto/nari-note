using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
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
        var author = new UserBuilder().WithName("author").WithProfileImage("https://images.test/author").Build();
        var liker = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("記事").WithBody("本文").WithTags("振り飛車").LikedBy(liker).Build();
        this.articleRepository.FindLatestSingleArticlesAsync(10, 20).Returns((new List<Article> { article }, 1));

        var response = await this.service.ExecuteAsync(new GetArticlesRequest { Limit = 10, Offset = 20 });

        var dto = Assert.Single(response.Articles);
        Assert.Equal(article.Id, dto.Id);
        Assert.Equal("記事", dto.Title);
        Assert.Equal("本文", dto.Body);
        Assert.Equal(author.Id, dto.AuthorId);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal("https://images.test/author", dto.UserIconImageUrl);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(TestTimeProvider.DefaultUtcNow.AddDays(-1), dto.PublishedAt);
    }

    [Fact]
    public async Task 記事が無ければ空の一覧を返す()
    {
        this.articleRepository.FindLatestSingleArticlesAsync(20, 0).Returns((new List<Article>(), 0));

        var response = await this.service.ExecuteAsync(new GetArticlesRequest());

        Assert.Empty(response.Articles);
    }
}
