using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UpdateArticleServiceTest
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().Build();
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly IKifuRepository kifuRepository = Substitute.For<IKifuRepository>();
    readonly UpdateArticleService service;

    public UpdateArticleServiceTest()
    {
        this.service = new UpdateArticleService(
            this.articleRepository,
            this.courseRepository,
            this.kifuRepository,
            new TestTimeProvider()
        );
    }

    Article SetupArticle(ArticleBuilder builder)
    {
        var article = builder.Build();
        this.articleRepository.FindForceByIdAsync(article.Id).Returns(article);
        return article;
    }

    [Fact]
    public async Task 指定した項目だけを更新し更新日時を現在時刻にする()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).WithTitle("旧タイトル").WithBody("旧本文"));
        var request = new UpdateArticleRequest { Id = article.Id, Title = "新タイトル", Tags = ["新タグ"] };

        var response = await this.service.ExecuteAsync(this.author.Id, request);

        Assert.Equal("新タイトル", article.Title);
        Assert.Equal("旧本文", article.Body);
        Assert.Equal(Now, article.UpdatedAt);
        Assert.Equal(Now, response.UpdatedAt);
        await this.articleRepository.Received(1).UpdateWithTagAsync(article, request.Tags);
    }

    [Fact]
    public async Task 下書きを公開すると現在時刻が公開日時になる()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).Draft());

        await this.service.ExecuteAsync(
            this.author.Id,
            new UpdateArticleRequest { Id = article.Id, IsPublished = true }
        );

        Assert.Equal(Now, article.PublishedAt);
    }

    [Fact]
    public async Task 公開済みの記事を更新しても公開日時は変わらない()
    {
        var publishedAt = Now.AddDays(-5);
        var article = SetupArticle(new ArticleBuilder(this.author).PublishedAt(publishedAt));

        await this.service.ExecuteAsync(
            this.author.Id,
            new UpdateArticleRequest { Id = article.Id, Title = "新タイトル", IsPublished = true }
        );

        Assert.Equal(publishedAt, article.PublishedAt);
    }

    [Fact]
    public async Task 公開日時が指定されていればその日時に変更する()
    {
        var scheduledAt = Now.AddDays(3);
        var article = SetupArticle(new ArticleBuilder(this.author).PublishedAt(Now.AddDays(-5)));

        await this.service.ExecuteAsync(
            this.author.Id,
            new UpdateArticleRequest { Id = article.Id, PublishedAt = scheduledAt }
        );

        Assert.Equal(scheduledAt, article.PublishedAt);
    }

    [Fact]
    public async Task 棋譜が指定されていれば置き換え_未指定なら変更しない()
    {
        var article = SetupArticle(new ArticleBuilder(this.author));

        await this.service.ExecuteAsync(this.author.Id, new UpdateArticleRequest { Id = article.Id, Title = "a" });
        await this.kifuRepository.DidNotReceiveWithAnyArgs().ReplaceAllByArticleIdAsync(article.Id, null!);

        await this.service.ExecuteAsync(this.author.Id, new UpdateArticleRequest
        {
            Id = article.Id,
            Kifus = [new KifuDto { Name = "第1局", KifuText = "▲7六歩", SortOrder = 0 }]
        });
        await this.kifuRepository.Received(1).ReplaceAllByArticleIdAsync(
            article.Id,
            Arg.Is<List<Kifu>>(kifus => kifus.Count == 1 && kifus[0].Name == "第1局" && kifus[0].ArticleId == article.Id)
        );
    }

    [Fact]
    public async Task 講座の記事は順序を変更できる()
    {
        var course = new CourseBuilder(this.author).Build();
        this.courseRepository.FindForceByIdAsync(course.Id).Returns(course);
        var article = SetupArticle(new ArticleBuilder(this.author).InCourse(course, 1));

        await this.service.ExecuteAsync(this.author.Id, new UpdateArticleRequest { Id = article.Id, ArticleOrder = 3 });

        Assert.Equal(3, article.ArticleOrder);
    }

    [Fact]
    public async Task 講座に属さない記事の順序は変更できない()
    {
        var article = SetupArticle(new ArticleBuilder(this.author));
        var request = new UpdateArticleRequest { Id = article.Id, ArticleOrder = 3 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.ExecuteAsync(this.author.Id, request));

        await this.articleRepository.DidNotReceiveWithAnyArgs().UpdateWithTagAsync(null!, null);
    }

    [Fact]
    public async Task 作者以外は更新できない()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).WithTitle("タイトル"));
        var otherUserId = UserId.From(Guid.CreateVersion7());
        var request = new UpdateArticleRequest { Id = article.Id, Title = "書き換え" };

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(otherUserId, request));

        Assert.Equal("タイトル", article.Title);
        await this.articleRepository.DidNotReceiveWithAnyArgs().UpdateWithTagAsync(null!, null);
    }

    [Fact]
    public async Task 他人の講座に属する記事は更新できない()
    {
        var course = new CourseBuilder(new UserBuilder().Build()).Build();
        this.courseRepository.FindForceByIdAsync(course.Id).Returns(course);
        var article = SetupArticle(new ArticleBuilder(this.author).InCourse(course, 1));
        var request = new UpdateArticleRequest { Id = article.Id, Title = "書き換え" };

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(this.author.Id, request));
    }
}
