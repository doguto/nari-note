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

public class CreateArticleServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().Build();
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly IKifuRepository kifuRepository = Substitute.For<IKifuRepository>();
    readonly CreateArticleService service;
    readonly TestTimeProvider timeProvider = new();

    public CreateArticleServiceTest()
    {
        // 実 Repository と同様に、保存時に ID が採番された Entity を返す
        this.articleRepository
            .CreateAsync(Arg.Any<Article>())
            .Returns(call =>
            {
                var article = call.Arg<Article>();
                article.Id = ArticleId.From(Guid.CreateVersion7());
                return article;
            });

        this.service = new CreateArticleService(
            this.articleRepository,
            this.courseRepository,
            this.kifuRepository,
            this.timeProvider
        );
    }

    CreateArticleRequest CreateRequest()
    {
        return new CreateArticleRequest
        {
            Title = "四間飛車の基本",
            Body = "本文",
            AuthorId = this.author.Id
        };
    }

    [Fact]
    public async Task 公開指定で公開日時が未指定なら現在時刻で公開される()
    {
        var request = CreateRequest();
        request.IsPublished = true;

        await this.service.ExecuteAsync(request);

        await this.articleRepository.Received(1).CreateAsync(
            Arg.Is<Article>(a => a.PublishedAt == TestTimeProvider.DefaultUtcNow)
        );
    }

    [Fact]
    public async Task 公開日時が指定されていればその日時で予約投稿される()
    {
        var scheduledAt = TestTimeProvider.DefaultUtcNow.AddDays(7);
        var request = CreateRequest();
        request.PublishedAt = scheduledAt;

        await this.service.ExecuteAsync(request);

        await this.articleRepository.Received(1).CreateAsync(Arg.Is<Article>(a => a.PublishedAt == scheduledAt));
    }

    [Fact]
    public async Task 公開指定が無ければ下書きとして保存される()
    {
        await this.service.ExecuteAsync(CreateRequest());

        await this.articleRepository.Received(1).CreateAsync(Arg.Is<Article>(a => a.PublishedAt == null));
    }

    [Fact]
    public async Task 入力内容と採番されたIDが記事に反映される()
    {
        var response = await this.service.ExecuteAsync(CreateRequest());

        await this.articleRepository.Received(1).CreateAsync(
            Arg.Is<Article>(a => a.Title == "四間飛車の基本" && a.Body == "本文" && a.AuthorId == this.author.Id)
        );
        Assert.NotEqual(Guid.Empty, response.Id.Value);
    }

    [Fact]
    public async Task 他人の講座には記事を追加できない()
    {
        var course = new CourseBuilder(new UserBuilder().Build()).Build();
        this.courseRepository.FindForceByIdAsync(course.Id).Returns(course);
        var request = CreateRequest();
        request.CourseId = course.Id;

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(request));

        await this.articleRepository.DidNotReceive().CreateAsync(Arg.Any<Article>());
    }

    [Fact]
    public async Task 自分の講座には記事を追加できる()
    {
        var course = new CourseBuilder(this.author).Build();
        this.courseRepository.FindForceByIdAsync(course.Id).Returns(course);
        var request = CreateRequest();
        request.CourseId = course.Id;
        request.ArticleOrder = 2;

        await this.service.ExecuteAsync(request);

        await this.articleRepository.Received(1).CreateAsync(
            Arg.Is<Article>(a => a.CourseId == course.Id && a.ArticleOrder == 2)
        );
    }

    [Fact]
    public async Task タグと棋譜が指定されていれば記事に紐づけて保存される()
    {
        var request = CreateRequest();
        request.Tags = ["振り飛車", "初心者"];
        request.Kifus = [new KifuDto { Name = "第1局", KifuText = "▲7六歩", SortOrder = 0 }];

        var response = await this.service.ExecuteAsync(request);

        await this.articleRepository.Received(1).UpdateWithTagAsync(
            Arg.Is<Article>(a => a.Id == response.Id),
            Arg.Is<List<string>>(tags => tags.SequenceEqual(new[] { "振り飛車", "初心者" }))
        );
        await this.kifuRepository.Received(1).ReplaceAllByArticleIdAsync(
            response.Id,
            Arg.Is<List<Kifu>>(kifus =>
                kifus.Count == 1 && kifus[0].ArticleId == response.Id && kifus[0].KifuText == "▲7六歩"
            )
        );
    }

    [Fact]
    public async Task タグと棋譜が無ければ保存処理を呼ばない()
    {
        await this.service.ExecuteAsync(CreateRequest());

        // Vogen の ID 型は Arg.Any が使えないため、WithAnyArgs で任意の引数にマッチさせる
        await this.articleRepository.DidNotReceiveWithAnyArgs().UpdateWithTagAsync(null!, null);
        await this.kifuRepository.DidNotReceiveWithAnyArgs().ReplaceAllByArticleIdAsync(
            ArticleId.From(Guid.CreateVersion7()),
            null!
        );
    }
}
