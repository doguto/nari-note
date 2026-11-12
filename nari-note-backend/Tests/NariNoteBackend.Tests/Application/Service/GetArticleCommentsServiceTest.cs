using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticleCommentsServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().WithName("author").Build();
    readonly ICommentRepository commentRepository = Substitute.For<ICommentRepository>();
    readonly User reader = new UserBuilder().WithName("reader").Build();
    readonly GetArticleCommentsService service;
    readonly TestTimeProvider timeProvider = new();

    public GetArticleCommentsServiceTest()
    {
        this.service = new GetArticleCommentsService(this.articleRepository, this.commentRepository, this.timeProvider);
    }

    Article SetupArticle(ArticleBuilder builder)
    {
        var article = builder.Build();
        this.articleRepository.FindForceByIdAsync(article.Id).Returns(article);
        return article;
    }

    [Fact]
    public async Task 指定した範囲のコメントと総件数を返す()
    {
        var article = SetupArticle(new ArticleBuilder(this.author));
        var comment = TestEntity.Comment(this.reader, article, "勉強になりました");
        comment.User = this.reader;
        this.commentRepository
            .FindPageByArticleAsync(article.Id, 20, 40)
            .Returns((new List<Comment> { comment }, 41));

        var response = await this.service.ExecuteAsync(new GetArticleCommentsRequest { Id = article.Id, Limit = 20, Offset = 40 });

        var dto = Assert.Single(response.Comments);
        Assert.Equal("勉強になりました", dto.Message);
        Assert.Equal("reader", dto.UserName);
        Assert.Equal(this.reader.Id, dto.UserId);
        Assert.Equal(41, response.TotalCount);
    }

    [Fact]
    public async Task 下書きのコメントは作者以外には存在しないものとして扱う()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).Draft());
        // Vogen の ID 型は Arg.Any が使えないため、ForAnyArgs で任意の引数にマッチさせる
        this.commentRepository
            .FindPageByArticleAsync(ArticleId.From(Guid.CreateVersion7()), 0, 0)
            .ReturnsForAnyArgs((new List<Comment>(), 0));
        var request = new GetArticleCommentsRequest { Id = article.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request, this.reader.Id));
        var response = await this.service.ExecuteAsync(request, this.author.Id);
        Assert.Empty(response.Comments);
    }
}
