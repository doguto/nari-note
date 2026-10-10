using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticleContentServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().Build();
    readonly ICommentRepository commentRepository = Substitute.For<ICommentRepository>();
    readonly ILikeRepository likeRepository = Substitute.For<ILikeRepository>();
    readonly User otherUser = new UserBuilder().Build();
    readonly GetArticleContentService service;
    readonly TestTimeProvider timeProvider = new();

    public GetArticleContentServiceTest()
    {
        // Vogen の ID 型は Arg.Any が使えないため、ForAnyArgs で任意の引数にマッチさせる
        this.commentRepository
            .FindByArticleAsync(ArticleId.From(Guid.CreateVersion7()))
            .ReturnsForAnyArgs(new List<Comment>());

        this.service = new GetArticleContentService(
            this.articleRepository,
            this.commentRepository,
            this.likeRepository,
            this.timeProvider
        );
    }

    Article SetupArticle(ArticleBuilder builder)
    {
        var article = builder.Build();
        this.articleRepository.FindForceByIdAsync(article.Id).Returns(article);
        return article;
    }

    [Fact]
    public async Task 公開済みの記事は未ログインでも取得できる()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).WithTitle("公開記事"));

        var response = await this.service.ExecuteAsync(new GetArticleContentRequest { Id = article.Id });

        Assert.Equal("公開記事", response.Article.Title);
        Assert.Equal(this.author.Name, response.Article.AuthorName);
        Assert.False(response.IsLiked);
    }

    [Fact]
    public async Task 下書きは作者以外には存在しないものとして扱う()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).Draft());
        var request = new GetArticleContentRequest { Id = article.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request, this.otherUser.Id));
    }

    [Fact]
    public async Task 下書きでも作者本人は取得できる()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).Draft());

        var response = await this.service.ExecuteAsync(new GetArticleContentRequest { Id = article.Id }, this.author.Id);

        Assert.False(response.Article.IsPublished);
    }

    [Fact]
    public async Task 予約投稿は公開日時になるまで作者以外は取得できない()
    {
        var publishAt = TestTimeProvider.DefaultUtcNow.AddHours(1);
        var article = SetupArticle(new ArticleBuilder(this.author).PublishedAt(publishAt));
        var request = new GetArticleContentRequest { Id = article.Id };

        // 公開日時の直前
        this.timeProvider.SetUtcNow(publishAt.AddTicks(-1));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        // 公開日時ちょうど
        this.timeProvider.SetUtcNow(publishAt);
        var response = await this.service.ExecuteAsync(request);
        Assert.Equal(article.Id, response.Article.Id);
    }

    [Fact]
    public async Task ログインユーザーがいいね済みならIsLikedがtrueになる()
    {
        var article = SetupArticle(new ArticleBuilder(this.author));
        this.likeRepository
            .FindByUserAndArticleAsync(this.otherUser.Id, article.Id)
            .Returns(new Like { UserId = this.otherUser.Id, ArticleId = article.Id });

        var response = await this.service.ExecuteAsync(
            new GetArticleContentRequest { Id = article.Id },
            this.otherUser.Id
        );

        Assert.True(response.IsLiked);
    }
}
