using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetLikedArticlesServiceTest
{
    readonly ILikeRepository likeRepository = Substitute.For<ILikeRepository>();
    readonly GetLikedArticlesService service;

    public GetLikedArticlesServiceTest()
    {
        this.service = new GetLikedArticlesService(this.likeRepository);
    }

    [Fact]
    public async Task ユーザーがいいねした記事を返す()
    {
        var user = new UserBuilder().Build();
        var author = new UserBuilder().WithName("author").Build();
        var first = new ArticleBuilder(author).WithTitle("1件目").WithTags("振り飛車").LikedBy(user, author).Build();
        var second = new ArticleBuilder(author).WithTitle("2件目").LikedBy(user).Build();
        this.likeRepository.FindLikedArticlesByUserAsync(user.Id).Returns(new List<Article> { first, second });

        var response = await this.service.ExecuteAsync(new GetLikedArticlesRequest { UserId = user.Id });

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(["1件目", "2件目"], response.Articles.Select(a => a.Title));
        Assert.Equal("author", response.Articles[0].AuthorName);
        Assert.Equal(["振り飛車"], response.Articles[0].Tags);
        Assert.Equal([2, 1], response.Articles.Select(a => a.LikeCount));
    }
}
