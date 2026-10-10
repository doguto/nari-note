using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class ArticlesControllerTest : IntegrationTestBase
{
    public ArticlesControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task 記事一覧は公開中の単体記事だけを新しい順に返す()
    {
        var now = TestTimeProvider.DefaultUtcNow;
        var author = new UserBuilder().Build();
        var course = new CourseBuilder(author).Build();
        await SeedAsync(
            author,
            course,
            new ArticleBuilder(author).WithTitle("古い記事").CreatedAt(now.AddDays(-3)).Build(),
            new ArticleBuilder(author).WithTitle("新しい記事").CreatedAt(now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("下書き").Draft().Build(),
            new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(now.AddDays(1)).Build(),
            new ArticleBuilder(author).WithTitle("講座の記事").InCourse(course).Build()
        );

        var response = await CreateClient().GetAsync("/api/articles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetArticlesResponse>(response);
        Assert.Equal(["新しい記事", "古い記事"], body.Articles.Select(a => a.Title));
        Assert.All(body.Articles, a => Assert.Equal(author.Name, a.AuthorName));
    }

    [Fact]
    public async Task 未認証では記事を作成できない()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/articles", new { title = "タイトル", body = "本文" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Articles.CountAsync()));
    }

    [Fact]
    public async Task 記事を作成するとログインユーザーを作者として保存される()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(author);

        var response = await CreateClientAs(author).PostAsJsonAsync("/api/articles", new
        {
            title = "四間飛車の基本",
            body = "本文",
            tags = new[] { "振り飛車" },
            isPublished = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<CreateArticleResponse>(response);
        var saved = await QueryAsync(db => db.Articles
                                             .Include(a => a.ArticleTags)
                                             .ThenInclude(at => at.Tag)
                                             .SingleAsync(a => a.Id == body.Id));
        Assert.Equal("四間飛車の基本", saved.Title);
        Assert.Equal(author.Id, saved.AuthorId);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, saved.PublishedAt);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, saved.CreatedAt);
        Assert.Equal(["振り飛車"], saved.ArticleTags.Select(at => at.Tag.Name));
    }

    [Fact]
    public async Task タイトルが空の記事は作成できない()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(author);

        var response = await CreateClientAs(author).PostAsJsonAsync("/api/articles", new { title = "", body = "本文" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Articles.CountAsync()));
    }

    [Fact]
    public async Task 下書きは作者本人だけが取得できる()
    {
        var author = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var draft = new ArticleBuilder(author).Draft().Build();
        await SeedAsync(author, otherUser, draft);
        var url = $"/api/articles/{draft.Id.Value}";

        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient().GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClientAs(otherUser).GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClientAs(author).GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task 予約投稿は公開日時を過ぎると取得できるようになる()
    {
        var publishAt = TestTimeProvider.DefaultUtcNow.AddHours(1);
        var author = new UserBuilder().Build();
        var scheduled = new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(publishAt).Build();
        await SeedAsync(author, scheduled);
        var url = $"/api/articles/{scheduled.Id.Value}";
        var client = CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);

        TimeProvider.SetUtcNow(publishAt);
        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetArticleContentResponse>(response);
        Assert.Equal("予約投稿", body.Article.Title);
    }

    [Fact]
    public async Task 存在しない記事は404を返す()
    {
        var response = await CreateClient().GetAsync($"/api/articles/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await ReadAsync<ErrorResponse>(response);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, body.TimeStamp);
    }
}
