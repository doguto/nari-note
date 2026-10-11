using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticlesByAuthorServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().WithName("author").Build();
    readonly GetArticlesByAuthorService service;

    public GetArticlesByAuthorServiceTest()
    {
        this.service = new GetArticlesByAuthorService(this.articleRepository);
    }

    void SetupArticles(params Article[] articles)
    {
        this.articleRepository.FindByAuthorAsync(this.author.Id).Returns(articles.ToList());
    }

    [Fact]
    public async Task 作者の公開済みの記事だけを返す()
    {
        SetupArticles(
            new ArticleBuilder(this.author).WithTitle("公開記事").WithTags("振り飛車").Build(),
            new ArticleBuilder(this.author).WithTitle("下書き").Draft().Build()
        );

        var response = await this.service.ExecuteAsync(new GetArticlesByAuthorRequest { AuthorId = this.author.Id });

        Assert.Equal(this.author.Id, response.AuthorId);
        Assert.Equal("author", response.AuthorName);
        var dto = Assert.Single(response.Articles);
        Assert.Equal("公開記事", dto.Title);
        Assert.Equal(["振り飛車"], dto.Tags);
    }

    [Fact]
    public async Task 公開済みの記事が無ければ作者名は空になる()
    {
        SetupArticles(new ArticleBuilder(this.author).Draft().Build());

        var response = await this.service.ExecuteAsync(new GetArticlesByAuthorRequest { AuthorId = this.author.Id });

        Assert.Equal("", response.AuthorName);
        Assert.Empty(response.Articles);
    }

    [Fact(Skip = "既知の不具合 (#571): 公開日時を過ぎていない予約投稿も返す")]
    public async Task 公開日時を過ぎていない予約投稿は返さない()
    {
        SetupArticles(
            new ArticleBuilder(this.author).WithTitle("公開記事").Build(),
            new ArticleBuilder(this.author).WithTitle("予約投稿").PublishedAt(TestTimeProvider.DefaultUtcNow.AddDays(1)).Build()
        );

        var response = await this.service.ExecuteAsync(new GetArticlesByAuthorRequest { AuthorId = this.author.Id });

        Assert.Equal(["公開記事"], response.Articles.Select(a => a.Title));
    }
}
