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
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    public ArticlesControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    #region GET /api/articles

    [Fact]
    public async Task 記事一覧は公開中の単体記事だけを新しい順に返す()
    {
        var author = new UserBuilder().Build();
        var course = new CourseBuilder(author).Build();
        await SeedAsync(
            author,
            course,
            new ArticleBuilder(author).WithTitle("古い記事").CreatedAt(Now.AddDays(-3)).Build(),
            new ArticleBuilder(author).WithTitle("新しい記事").CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("下書き").Draft().Build(),
            new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(Now.AddDays(1)).Build(),
            new ArticleBuilder(author).WithTitle("講座の記事").InCourse(course).Build()
        );

        var response = await CreateClient().GetAsync("/api/articles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetArticlesResponse>(response);
        Assert.Equal(["新しい記事", "古い記事"], body.Articles.Select(a => a.Title));
        Assert.All(body.Articles, a => Assert.Equal(author.Name, a.AuthorName));
    }

    [Fact]
    public async Task 記事一覧はlimitとoffsetでページングできる()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(
            author,
            new ArticleBuilder(author).WithTitle("1番目").CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("2番目").CreatedAt(Now.AddDays(-2)).Build(),
            new ArticleBuilder(author).WithTitle("3番目").CreatedAt(Now.AddDays(-3)).Build()
        );

        var response = await CreateClient().GetAsync("/api/articles?limit=1&offset=1");

        var body = await ReadAsync<GetArticlesResponse>(response);
        Assert.Equal(["2番目"], body.Articles.Select(a => a.Title));
    }

    #endregion

    #region POST /api/articles

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
            kifus = new[] { new { name = "第1局", kifuText = "▲7六歩", sortOrder = 0 } },
            isPublished = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<CreateArticleResponse>(response);
        var saved = await QueryAsync(db => db.Articles
                                             .Include(a => a.ArticleTags)
                                             .ThenInclude(at => at.Tag)
                                             .Include(a => a.Kifus)
                                             .SingleAsync(a => a.Id == body.Id));
        Assert.Equal("四間飛車の基本", saved.Title);
        Assert.Equal(author.Id, saved.AuthorId);
        Assert.Equal(Now, saved.PublishedAt);
        Assert.Equal(Now, saved.CreatedAt);
        Assert.Equal(["振り飛車"], saved.ArticleTags.Select(at => at.Tag.Name));
        Assert.Equal(["第1局"], saved.Kifus.Select(k => k.Name));
    }

    [Fact]
    public async Task 既存のタグは再利用して記事に紐づける()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(author, TestEntity.Tag("振り飛車"));

        await CreateClientAs(author).PostAsJsonAsync("/api/articles", new
        {
            title = "タイトル",
            body = "本文",
            tags = new[] { "振り飛車", "初心者" }
        });

        Assert.Equal(["初心者", "振り飛車"], await QueryAsync(db => db.Tags.Select(t => t.Name).Order().ToListAsync()));
        Assert.Equal(2, await QueryAsync(db => db.ArticleTags.CountAsync()));
    }

    [Fact]
    public async Task 他人の講座には記事を追加できない()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Build();
        await SeedAsync(owner, otherUser, course);

        var response = await CreateClientAs(otherUser).PostAsJsonAsync("/api/articles", new
        {
            title = "タイトル",
            body = "本文",
            courseId = course.Id.Value
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Articles.CountAsync()));
    }

    [Theory]
    [InlineData("", "本文", "振り飛車")]
    [InlineData("タイトル", "", "振り飛車")]
    [InlineData("タイトル", "本文", "不正な タグ")]
    public async Task 入力が不正な記事は作成できない(string title, string body, string tag)
    {
        var author = new UserBuilder().Build();
        await SeedAsync(author);

        var response = await CreateClientAs(author).PostAsJsonAsync(
            "/api/articles",
            new { title, body, tags = new[] { tag } }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Articles.CountAsync()));
    }

    #endregion

    #region GET /api/articles/{id}

    [Fact]
    public async Task 記事の内容をタグ_棋譜_コメント_いいねと合わせて取得できる()
    {
        var author = new UserBuilder().WithName("author").Build();
        var reader = new UserBuilder().WithName("reader").Build();
        var course = new CourseBuilder(author).WithName("四間飛車講座").Build();
        var article = new ArticleBuilder(author).WithTitle("記事").WithBody("本文").InCourse(course, 1).Build();
        var tag = TestEntity.Tag("振り飛車");
        await SeedAsync(
            author,
            reader,
            course,
            article,
            tag,
            TestEntity.ArticleTag(article, tag),
            TestEntity.Kifu(article, "第1局"),
            TestEntity.Like(reader, article),
            TestEntity.Comment(reader, article, "2件目", Now.AddHours(-1)),
            TestEntity.Comment(author, article, "1件目", Now.AddHours(-2))
        );
        var url = $"/api/articles/{article.Id.Value}";

        var body = await ReadAsync<GetArticleContentResponse>(await CreateClientAs(reader).GetAsync(url));

        Assert.Equal("記事", body.Article.Title);
        Assert.Equal("本文", body.Article.Body);
        Assert.Equal("author", body.Article.AuthorName);
        Assert.Equal(["振り飛車"], body.Article.Tags);
        Assert.Equal(["第1局"], body.Article.Kifus.Select(k => k.Name));
        Assert.Equal(1, body.Article.LikeCount);
        Assert.True(body.IsLiked);
        Assert.Equal(course.Id, body.CourseId);
        Assert.Equal("四間飛車講座", body.CourseName);
        Assert.Equal(["1件目", "2件目"], body.Comments.Select(c => c.Message));
        Assert.Equal(["author", "reader"], body.Comments.Select(c => c.UserName));

        // 未ログインの場合はいいね済みにならない
        var anonymous = await ReadAsync<GetArticleContentResponse>(await CreateClient().GetAsync(url));
        Assert.False(anonymous.IsLiked);
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
        var publishAt = Now.AddHours(1);
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
        Assert.Equal(Now, body.TimeStamp);
    }

    #endregion

    #region PUT /api/articles/{id}

    [Fact]
    public async Task 作者は記事のタイトル_本文_タグを更新できる()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("旧タイトル").PublishedAt(Now.AddDays(-5)).Build();
        var oldTag = TestEntity.Tag("旧タグ");
        await SeedAsync(author, article, oldTag, TestEntity.ArticleTag(article, oldTag));

        var response = await CreateClientAs(author).PutAsJsonAsync($"/api/articles/{article.Id.Value}", new
        {
            title = "新タイトル",
            body = "新本文",
            tags = new[] { "新タグ" }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Now, (await ReadAsync<UpdateArticleResponse>(response)).UpdatedAt);
        var saved = await QueryAsync(db => db.Articles
                                             .Include(a => a.ArticleTags)
                                             .ThenInclude(at => at.Tag)
                                             .SingleAsync());
        Assert.Equal("新タイトル", saved.Title);
        Assert.Equal("新本文", saved.Body);
        Assert.Equal(["新タグ"], saved.ArticleTags.Select(at => at.Tag.Name));
        Assert.Equal(Now, saved.UpdatedAt);
        // 公開済みの記事の公開日時は変わらない
        Assert.Equal(Now.AddDays(-5), saved.PublishedAt);
    }

    [Fact]
    public async Task 指定しなかった項目は更新されない()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("タイトル").WithBody("本文").Build();
        var tag = TestEntity.Tag("タグ");
        await SeedAsync(author, article, tag, TestEntity.ArticleTag(article, tag), TestEntity.Kifu(article, "第1局"));

        var response = await CreateClientAs(author).PutAsJsonAsync(
            $"/api/articles/{article.Id.Value}",
            new { title = "新タイトル" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await QueryAsync(db => db.Articles
                                             .Include(a => a.ArticleTags)
                                             .Include(a => a.Kifus)
                                             .SingleAsync());
        Assert.Equal("新タイトル", saved.Title);
        Assert.Equal("本文", saved.Body);
        Assert.Single(saved.ArticleTags);
        Assert.Single(saved.Kifus);
    }

    [Fact]
    public async Task 下書きを公開すると現在時刻が公開日時になる()
    {
        var author = new UserBuilder().Build();
        var draft = new ArticleBuilder(author).Draft().Build();
        await SeedAsync(author, draft);

        var response = await CreateClientAs(author).PutAsJsonAsync(
            $"/api/articles/{draft.Id.Value}",
            new { isPublished = true }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Now, (await QueryAsync(db => db.Articles.SingleAsync())).PublishedAt);
    }

    [Fact]
    public async Task 公開日時を指定すると予約投稿になる()
    {
        var scheduledAt = Now.AddDays(3);
        var author = new UserBuilder().Build();
        var draft = new ArticleBuilder(author).Draft().Build();
        await SeedAsync(author, draft);

        var response = await CreateClientAs(author).PutAsJsonAsync(
            $"/api/articles/{draft.Id.Value}",
            new { isPublished = true, publishedAt = scheduledAt }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(scheduledAt, (await QueryAsync(db => db.Articles.SingleAsync())).PublishedAt);
    }

    [Fact]
    public async Task 棋譜を増やして更新できる()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article, TestEntity.Kifu(article, "旧第1局"));

        var response = await CreateClientAs(author).PutAsJsonAsync($"/api/articles/{article.Id.Value}", new
        {
            kifus = new[]
            {
                new { name = "第1局", kifuText = "▲7六歩", sortOrder = 0 },
                new { name = "第2局", kifuText = "▲2六歩", sortOrder = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var kifus = await QueryAsync(db => db.Kifus.OrderBy(k => k.SortOrder).ToListAsync());
        Assert.Equal(["第1局", "第2局"], kifus.Select(k => k.Name));
    }

    [Fact(Skip = "既知の不具合: KifuRepository.ReplaceAllByArticleIdAsync が余分な既存の棋譜を削除しない")]
    public async Task 棋譜を減らして更新すると余分な棋譜は削除される()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article, TestEntity.Kifu(article, "旧第1局", 0), TestEntity.Kifu(article, "旧第2局", 1));

        var response = await CreateClientAs(author).PutAsJsonAsync($"/api/articles/{article.Id.Value}", new
        {
            kifus = new[] { new { name = "第1局", kifuText = "▲7六歩", sortOrder = 0 } }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["第1局"], await QueryAsync(db => db.Kifus.Select(k => k.Name).ToListAsync()));
    }

    [Fact]
    public async Task 講座に属さない記事の順序は変更できない()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article);

        var response = await CreateClientAs(author).PutAsJsonAsync(
            $"/api/articles/{article.Id.Value}",
            new { articleOrder = 2 }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 作者以外は記事を更新できない()
    {
        var author = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var article = new ArticleBuilder(author).WithTitle("タイトル").Build();
        await SeedAsync(author, otherUser, article);
        var url = $"/api/articles/{article.Id.Value}";
        var payload = new { title = "書き換え" };

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().PutAsJsonAsync(url, payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClientAs(otherUser).PutAsJsonAsync(url, payload)).StatusCode);
        Assert.Equal("タイトル", (await QueryAsync(db => db.Articles.SingleAsync())).Title);
    }

    [Fact]
    public async Task 存在しない記事は更新できない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync(
            $"/api/articles/{Guid.CreateVersion7()}",
            new { title = "タイトル" }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DELETE /api/articles/{id}

    [Fact]
    public async Task 作者は記事を削除できる()
    {
        var author = new UserBuilder().Build();
        var reader = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(
            author,
            reader,
            article,
            TestEntity.Like(reader, article),
            TestEntity.Comment(reader, article, "コメント"),
            TestEntity.Kifu(article, "第1局")
        );

        var response = await CreateClientAs(author).DeleteAsync($"/api/articles/{article.Id.Value}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Articles.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Likes.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Comments.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Kifus.CountAsync()));
    }

    [Fact]
    public async Task 作者以外は記事を削除できない()
    {
        var author = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, otherUser, article);
        var url = $"/api/articles/{article.Id.Value}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClientAs(otherUser).DeleteAsync(url)).StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.Articles.CountAsync()));
    }

    [Fact]
    public async Task 存在しない記事は削除できない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).DeleteAsync($"/api/articles/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GET /api/articles/author/{authorId}

    [Fact]
    public async Task 作者別の記事一覧は公開済みの記事だけを新しい順に返す()
    {
        var author = new UserBuilder().WithName("author").Build();
        var otherUser = new UserBuilder().Build();
        await SeedAsync(
            author,
            otherUser,
            new ArticleBuilder(author).WithTitle("古い記事").CreatedAt(Now.AddDays(-3)).Build(),
            new ArticleBuilder(author).WithTitle("新しい記事").CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("下書き").Draft().Build(),
            new ArticleBuilder(otherUser).WithTitle("他人の記事").Build()
        );

        var response = await CreateClient().GetAsync($"/api/articles/author/{author.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetArticlesByAuthorResponse>(response);
        Assert.Equal(author.Id, body.AuthorId);
        Assert.Equal("author", body.AuthorName);
        Assert.Equal(["新しい記事", "古い記事"], body.Articles.Select(a => a.Title));
    }

    [Fact(Skip = "既知の不具合: GetArticlesByAuthorService が公開日時を過ぎていない予約投稿も返す")]
    public async Task 作者別の記事一覧に予約投稿は含まれない()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(
            author,
            new ArticleBuilder(author).WithTitle("公開記事").Build(),
            new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(Now.AddDays(1)).Build()
        );

        var response = await CreateClient().GetAsync($"/api/articles/author/{author.Id.Value}");

        var body = await ReadAsync<GetArticlesByAuthorResponse>(response);
        Assert.Equal(["公開記事"], body.Articles.Select(a => a.Title));
    }

    #endregion

    #region GET /api/articles/tag/{tagName}

    [Fact]
    public async Task タグ別の記事一覧は公開中の記事だけを返す()
    {
        var author = new UserBuilder().Build();
        var tag = TestEntity.Tag("Shogi");
        var otherTag = TestEntity.Tag("囲碁");
        var published = new ArticleBuilder(author).WithTitle("公開記事").Build();
        var draft = new ArticleBuilder(author).WithTitle("下書き").Draft().Build();
        var scheduled = new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(Now.AddDays(1)).Build();
        var otherTagged = new ArticleBuilder(author).WithTitle("別タグの記事").Build();
        await SeedAsync(
            author,
            tag,
            otherTag,
            published,
            draft,
            scheduled,
            otherTagged,
            TestEntity.ArticleTag(published, tag),
            TestEntity.ArticleTag(draft, tag),
            TestEntity.ArticleTag(scheduled, tag),
            TestEntity.ArticleTag(otherTagged, otherTag)
        );

        // タグ名の大文字小文字は区別しない
        var response = await CreateClient().GetAsync("/api/articles/tag/shogi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetArticlesByTagResponse>(response);
        Assert.Equal(["公開記事"], body.Articles.Select(a => a.Title));
        Assert.Equal(["Shogi"], body.Articles[0].Tags);
    }

    [Fact]
    public async Task 使用できない文字を含むタグ名は400を返す()
    {
        var response = await CreateClient().GetAsync($"/api/articles/tag/{Uri.EscapeDataString("不正な タグ")}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/articles/{id}/like

    [Fact]
    public async Task いいねは押すたびに付与と解除が切り替わる()
    {
        var author = new UserBuilder().Build();
        var reader = new UserBuilder().Build();
        var otherReader = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, reader, otherReader, article, TestEntity.Like(otherReader, article));
        var client = CreateClientAs(reader);
        var url = $"/api/articles/{article.Id.Value}/like";

        var liked = await ReadAsync<ToggleLikeResponse>(await client.PostAsync(url, null));
        Assert.True(liked.IsLiked);
        Assert.Equal(2, liked.CurrentLikeCount);

        var unliked = await ReadAsync<ToggleLikeResponse>(await client.PostAsync(url, null));
        Assert.False(unliked.IsLiked);
        Assert.Equal(1, unliked.CurrentLikeCount);
        Assert.Equal([otherReader.Id], await QueryAsync(db => db.Likes.Select(l => l.UserId).ToListAsync()));
    }

    [Fact]
    public async Task 未認証ではいいねできない()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article);

        var response = await CreateClient().PostAsync($"/api/articles/{article.Id.Value}/like", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/articles/my

    [Fact]
    public async Task 自分の記事一覧は下書きと予約投稿も含めて返す()
    {
        var author = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        await SeedAsync(
            author,
            otherUser,
            new ArticleBuilder(author).WithTitle("公開記事").CreatedAt(Now.AddDays(-3)).Build(),
            new ArticleBuilder(author).WithTitle("下書き").Draft().CreatedAt(Now.AddDays(-2)).Build(),
            new ArticleBuilder(author).WithTitle("予約投稿").PublishedAt(Now.AddDays(1)).CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(otherUser).WithTitle("他人の記事").Build()
        );

        var response = await CreateClientAs(author).GetAsync("/api/articles/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetMyArticlesResponse>(response);
        Assert.Equal(["予約投稿", "下書き", "公開記事"], body.Articles.Select(a => a.Title));
    }

    #endregion

    #region GET /api/articles/drafts

    [Fact]
    public async Task 下書き一覧は自分の下書きだけを返す()
    {
        var author = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        await SeedAsync(
            author,
            otherUser,
            new ArticleBuilder(author).WithTitle("古い下書き").Draft().CreatedAt(Now.AddDays(-3)).Build(),
            new ArticleBuilder(author).WithTitle("新しい下書き").Draft().CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("公開記事").Build(),
            new ArticleBuilder(otherUser).WithTitle("他人の下書き").Draft().Build()
        );

        var response = await CreateClientAs(author).GetAsync("/api/articles/drafts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetDraftArticlesResponse>(response);
        Assert.Equal(["新しい下書き", "古い下書き"], body.Articles.Select(a => a.Title));
    }

    [Theory]
    [InlineData("/api/articles/my")]
    [InlineData("/api/articles/drafts")]
    public async Task 未認証では自分の記事一覧を取得できない(string url)
    {
        var response = await CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/articles/search

    [Fact]
    public async Task タイトルか本文にキーワードを含む公開中の記事を検索できる()
    {
        var author = new UserBuilder().Build();
        await SeedAsync(
            author,
            new ArticleBuilder(author).WithTitle("四間飛車の基本").CreatedAt(Now.AddDays(-1)).Build(),
            new ArticleBuilder(author).WithTitle("序盤の考え方").WithBody("四間飛車を例に解説").CreatedAt(Now.AddDays(-2)).Build(),
            new ArticleBuilder(author).WithTitle("居飛車入門").Build(),
            new ArticleBuilder(author).WithTitle("四間飛車の下書き").Draft().Build(),
            new ArticleBuilder(author).WithTitle("四間飛車の予約投稿").PublishedAt(Now.AddDays(1)).Build()
        );

        var response = await CreateClient().GetAsync($"/api/articles/search?keyword={Uri.EscapeDataString("四間飛車")}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<SearchArticlesResponse>(response);
        Assert.Equal(["四間飛車の基本", "序盤の考え方"], body.Articles.Select(a => a.Title));
    }

    [Theory]
    [InlineData("/api/articles/search")]
    [InlineData("/api/articles/search?keyword=a&limit=0")]
    [InlineData("/api/articles/search?keyword=a&limit=101")]
    [InlineData("/api/articles/search?keyword=a&offset=-1")]
    public async Task 検索条件が不正な場合は400を返す(string url)
    {
        var response = await CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/articles/{id}/comments

    [Fact]
    public async Task 記事にコメントを投稿できる()
    {
        var author = new UserBuilder().Build();
        var reader = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, reader, article);

        var response = await CreateClientAs(reader).PostAsJsonAsync(
            $"/api/articles/{article.Id.Value}/comments",
            new { message = "勉強になりました" }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(Now, (await ReadAsync<CreateCommentResponse>(response)).CreatedAt);
        var saved = await QueryAsync(db => db.Comments.SingleAsync());
        Assert.Equal("勉強になりました", saved.Message);
        Assert.Equal(reader.Id, saved.UserId);
        Assert.Equal(article.Id, saved.ArticleId);
    }

    [Fact]
    public async Task 空のコメントや長すぎるコメントは投稿できない()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article);
        var client = CreateClientAs(author);
        var url = $"/api/articles/{article.Id.Value}/comments";

        var empty = await client.PostAsJsonAsync(url, new { message = "" });
        var tooLong = await client.PostAsJsonAsync(url, new { message = new string('あ', 1001) });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Comments.CountAsync()));
    }

    [Fact]
    public async Task 存在しない記事にはコメントできない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PostAsJsonAsync(
            $"/api/articles/{Guid.CreateVersion7()}/comments",
            new { message = "コメント" }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task 未認証ではコメントできない()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        await SeedAsync(author, article);

        var response = await CreateClient().PostAsJsonAsync(
            $"/api/articles/{article.Id.Value}/comments",
            new { message = "コメント" }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
