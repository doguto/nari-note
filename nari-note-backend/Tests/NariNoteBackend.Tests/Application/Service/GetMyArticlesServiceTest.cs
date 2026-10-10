using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetMyArticlesServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly GetMyArticlesService service;

    public GetMyArticlesServiceTest()
    {
        this.service = new GetMyArticlesService(this.articleRepository);
    }

    [Fact]
    public async Task 自分の記事を下書きも含めて返す()
    {
        var author = new UserBuilder().WithName("author").Build();
        var liker = new UserBuilder().Build();
        var published = new ArticleBuilder(author).WithTitle("公開記事").WithTags("振り飛車").LikedBy(liker).Build();
        var draft = new ArticleBuilder(author).WithTitle("下書き").Draft().Build();
        this.articleRepository.FindByAuthorAsync(author.Id).Returns(new List<Article> { published, draft });

        var response = await this.service.ExecuteAsync(author.Id);

        Assert.Equal(["公開記事", "下書き"], response.Articles.Select(a => a.Title));
        Assert.Equal([true, false], response.Articles.Select(a => a.IsPublished));
        var dto = response.Articles[0];
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
        Assert.Equal(published.PublishedAt, dto.PublishedAt);
    }
}
