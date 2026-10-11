using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
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
        var author = new UserBuilder().WithName("author").Build();
        var liker = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("四間飛車の基本").WithTags("振り飛車").LikedBy(liker).Build();
        this.articleRepository.SearchAsync("四間飛車", 10, 20).Returns(new List<Article> { article });

        var response = await this.service.ExecuteAsync(
            new SearchArticlesRequest { Keyword = "四間飛車", Limit = 10, Offset = 20 }
        );

        var dto = Assert.Single(response.Articles);
        Assert.Equal(article.Id, dto.Id);
        Assert.Equal("四間飛車の基本", dto.Title);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.Equal(1, dto.LikeCount);
    }
}
