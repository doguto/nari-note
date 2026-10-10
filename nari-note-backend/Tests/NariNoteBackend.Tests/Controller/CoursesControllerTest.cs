using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class CoursesControllerTest : IntegrationTestBase
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    public CoursesControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    #region GET /api/courses

    [Fact]
    public async Task 講座一覧は公開中の講座だけを新しい順に返す()
    {
        var owner = new UserBuilder().WithName("owner").Build();
        var liker = new UserBuilder().Build();
        var newCourse = new CourseBuilder(owner).WithName("新しい講座").CreatedAt(Now.AddDays(-1)).Build();
        await SeedAsync(
            owner,
            liker,
            newCourse,
            new CourseBuilder(owner).WithName("古い講座").CreatedAt(Now.AddDays(-3)).Build(),
            new CourseBuilder(owner).WithName("下書きの講座").Draft().Build(),
            new CourseBuilder(owner).WithName("予約公開の講座").PublishedAt(Now.AddDays(1)).Build(),
            new ArticleBuilder(owner).WithTitle("公開記事").InCourse(newCourse, 1).Build(),
            new ArticleBuilder(owner).WithTitle("下書き記事").InCourse(newCourse, 2).Draft().Build(),
            TestEntity.CourseLike(liker, newCourse)
        );

        var response = await CreateClient().GetAsync("/api/courses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetCoursesResponse>(response);
        Assert.Equal(["新しい講座", "古い講座"], body.Courses.Select(c => c.Name));
        var course = body.Courses[0];
        Assert.Equal("owner", course.UserName);
        Assert.Equal(1, course.LikeCount);
        // 未公開の記事は含めない
        Assert.Equal(["公開記事"], course.ArticleNames);
    }

    [Fact]
    public async Task 講座一覧はlimitとoffsetでページングできる()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(
            owner,
            new CourseBuilder(owner).WithName("1番目").CreatedAt(Now.AddDays(-1)).Build(),
            new CourseBuilder(owner).WithName("2番目").CreatedAt(Now.AddDays(-2)).Build(),
            new CourseBuilder(owner).WithName("3番目").CreatedAt(Now.AddDays(-3)).Build()
        );

        var response = await CreateClient().GetAsync("/api/courses?limit=1&offset=1");

        var body = await ReadAsync<GetCoursesResponse>(response);
        Assert.Equal(["2番目"], body.Courses.Select(c => c.Name));
    }

    #endregion

    #region GET /api/courses/search

    [Fact]
    public async Task 講座名か記事タイトルにキーワードを含む公開中の講座を検索できる()
    {
        var owner = new UserBuilder().Build();
        var matchedByArticle = new CourseBuilder(owner).WithName("序盤講座").CreatedAt(Now.AddDays(-2)).Build();
        await SeedAsync(
            owner,
            matchedByArticle,
            new CourseBuilder(owner).WithName("四間飛車講座").CreatedAt(Now.AddDays(-1)).Build(),
            new CourseBuilder(owner).WithName("居飛車講座").Build(),
            new CourseBuilder(owner).WithName("四間飛車の下書き講座").Draft().Build(),
            new ArticleBuilder(owner).WithTitle("四間飛車の組み方").InCourse(matchedByArticle, 1).Build()
        );

        var response = await CreateClient().GetAsync($"/api/courses/search?keyword={Uri.EscapeDataString("四間飛車")}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<SearchCoursesResponse>(response);
        Assert.Equal(["四間飛車講座", "序盤講座"], body.Courses.Select(c => c.Name));
    }

    [Fact]
    public async Task キーワードが無い講座検索は400を返す()
    {
        var response = await CreateClient().GetAsync("/api/courses/search");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/courses/my

    [Fact]
    public async Task 自分の講座一覧は下書きも含めて返す()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var draftCourse = new CourseBuilder(owner).WithName("下書きの講座").Draft().CreatedAt(Now.AddDays(-1)).Build();
        await SeedAsync(
            owner,
            otherUser,
            draftCourse,
            new CourseBuilder(owner).WithName("公開中の講座").CreatedAt(Now.AddDays(-2)).Build(),
            new CourseBuilder(otherUser).WithName("他人の講座").Build(),
            new ArticleBuilder(owner).WithTitle("下書き記事").InCourse(draftCourse, 1).Draft().Build()
        );

        var response = await CreateClientAs(owner).GetAsync("/api/courses/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetMyCoursesResponse>(response);
        Assert.Equal(["下書きの講座", "公開中の講座"], body.Courses.Select(c => c.Name));
        Assert.Equal(["下書き記事"], body.Courses[0].ArticleNames);
    }

    [Fact]
    public async Task 未認証では自分の講座一覧を取得できない()
    {
        var response = await CreateClient().GetAsync("/api/courses/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/courses/author/{authorId}

    [Fact]
    public async Task 作者別の講座一覧は公開中の講座だけを返す()
    {
        var owner = new UserBuilder().WithName("owner").Build();
        var otherUser = new UserBuilder().Build();
        await SeedAsync(
            owner,
            otherUser,
            new CourseBuilder(owner).WithName("古い講座").CreatedAt(Now.AddDays(-3)).Build(),
            new CourseBuilder(owner).WithName("新しい講座").CreatedAt(Now.AddDays(-1)).Build(),
            new CourseBuilder(owner).WithName("下書きの講座").Draft().Build(),
            new CourseBuilder(owner).WithName("予約公開の講座").PublishedAt(Now.AddDays(1)).Build(),
            new CourseBuilder(otherUser).WithName("他人の講座").Build()
        );

        var response = await CreateClient().GetAsync($"/api/courses/author/{owner.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetCoursesByAuthorResponse>(response);
        Assert.Equal(owner.Id, body.AuthorId);
        Assert.Equal("owner", body.AuthorName);
        Assert.Equal(["新しい講座", "古い講座"], body.Courses.Select(c => c.Name));
    }

    [Fact(Skip = "既知の不具合: CourseRepository.FindPublishedByAuthorAsync が未公開の記事も含めて返す")]
    public async Task 作者別の講座一覧に未公開の記事は含まれない()
    {
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Build();
        await SeedAsync(
            owner,
            course,
            new ArticleBuilder(owner).WithTitle("公開記事").InCourse(course, 1).Build(),
            new ArticleBuilder(owner).WithTitle("下書き記事").InCourse(course, 2).Draft().Build()
        );

        var response = await CreateClient().GetAsync($"/api/courses/author/{owner.Id.Value}");

        var body = await ReadAsync<GetCoursesByAuthorResponse>(response);
        Assert.Equal(["公開記事"], Assert.Single(body.Courses).ArticleNames);
    }

    #endregion

    #region POST /api/courses

    [Fact]
    public async Task 講座を作成すると下書きとして保存される()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);

        var response = await CreateClientAs(owner).PostAsJsonAsync("/api/courses", new { name = "四間飛車講座" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<CreateCourseResponse>(response);
        Assert.Equal("四間飛車講座", body.Course.Name);
        Assert.False(body.Course.IsPublished);
        var saved = await QueryAsync(db => db.Courses.SingleAsync());
        Assert.Equal(body.Course.Id, saved.Id);
        Assert.Equal(owner.Id, saved.UserId);
        Assert.Null(saved.PublishedAt);
        Assert.Equal(Now, saved.CreatedAt);
    }

    [Fact]
    public async Task 講座名が空や長すぎる場合は作成できない()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var client = CreateClientAs(owner);

        var empty = await client.PostAsJsonAsync("/api/courses", new { name = "" });
        var tooLong = await client.PostAsJsonAsync("/api/courses", new { name = new string('あ', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Courses.CountAsync()));
    }

    [Fact]
    public async Task 未認証では講座を作成できない()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/courses", new { name = "講座" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/courses/{id}

    [Fact]
    public async Task 講座の内容は公開中の記事だけを順序どおりに返す()
    {
        var owner = new UserBuilder().WithName("owner").Build();
        var course = new CourseBuilder(owner).WithName("四間飛車講座").Build();
        await SeedAsync(
            owner,
            course,
            new ArticleBuilder(owner).WithTitle("第2回").InCourse(course, 2).Build(),
            new ArticleBuilder(owner).WithTitle("第1回").InCourse(course, 1).Build(),
            new ArticleBuilder(owner).WithTitle("第3回（下書き）").InCourse(course, 3).Draft().Build(),
            new ArticleBuilder(owner).WithTitle("第4回（予約投稿）").InCourse(course, 4).PublishedAt(Now.AddDays(1)).Build()
        );

        var response = await CreateClient().GetAsync($"/api/courses/{course.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetCourseContentResponse>(response);
        Assert.Equal("四間飛車講座", body.Name);
        Assert.Equal("owner", body.UserName);
        Assert.True(body.IsPublished);
        Assert.Equal(["第1回", "第2回"], body.Articles.Select(a => a.Title));
    }

    [Fact]
    public async Task 下書きの講座は作成者本人だけが取得できる()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Draft().Build();
        await SeedAsync(owner, otherUser, course);
        var url = $"/api/courses/{course.Id.Value}";

        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient().GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClientAs(otherUser).GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClientAs(owner).GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task 予約公開の講座は公開日時を過ぎると取得できるようになる()
    {
        var publishAt = Now.AddHours(1);
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).PublishedAt(publishAt).Build();
        await SeedAsync(owner, course);
        var url = $"/api/courses/{course.Id.Value}";
        var client = CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);

        TimeProvider.SetUtcNow(publishAt);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task 存在しない講座は404を返す()
    {
        var response = await CreateClient().GetAsync($"/api/courses/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GET /api/courses/{id}/for-edit

    [Fact]
    public async Task 編集用の講座内容は未公開の記事も含めて返す()
    {
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Draft().Build();
        await SeedAsync(
            owner,
            course,
            new ArticleBuilder(owner).WithTitle("第2回（下書き）").InCourse(course, 2).Draft().Build(),
            new ArticleBuilder(owner).WithTitle("第1回").InCourse(course, 1).Build()
        );

        var response = await CreateClientAs(owner).GetAsync($"/api/courses/{course.Id.Value}/for-edit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetCourseContentResponse>(response);
        Assert.Equal(["第1回", "第2回（下書き）"], body.Articles.Select(a => a.Title));
        Assert.Equal([true, false], body.Articles.Select(a => a.IsPublished));
    }

    [Fact]
    public async Task 作成者以外は編集用の講座内容を取得できない()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Build();
        await SeedAsync(owner, otherUser, course);
        var url = $"/api/courses/{course.Id.Value}/for-edit";

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClientAs(otherUser).GetAsync(url)).StatusCode);
    }

    #endregion

    #region PUT /api/courses/{id}

    [Fact]
    public async Task 作成者は講座名を更新できる()
    {
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).WithName("旧講座名").PublishedAt(Now.AddDays(-5)).Build();
        await SeedAsync(owner, course);

        var response = await CreateClientAs(owner).PutAsJsonAsync(
            $"/api/courses/{course.Id.Value}",
            new { name = "新講座名" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Now, (await ReadAsync<UpdateCourseResponse>(response)).UpdatedAt);
        var saved = await QueryAsync(db => db.Courses.SingleAsync());
        Assert.Equal("新講座名", saved.Name);
        Assert.Equal(Now, saved.UpdatedAt);
        // 公開済みの講座の公開日時は変わらない
        Assert.Equal(Now.AddDays(-5), saved.PublishedAt);
    }

    [Fact]
    public async Task 講座を公開すると下書きの記事も同時に公開される()
    {
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Draft().Build();
        await SeedAsync(
            owner,
            course,
            new ArticleBuilder(owner).WithTitle("下書き記事").InCourse(course, 1).Draft().Build(),
            new ArticleBuilder(owner).WithTitle("公開済み記事").InCourse(course, 2).PublishedAt(Now.AddDays(-5)).Build()
        );

        var response = await CreateClientAs(owner).PutAsJsonAsync(
            $"/api/courses/{course.Id.Value}",
            new { isPublished = true }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Now, (await QueryAsync(db => db.Courses.SingleAsync())).PublishedAt);
        var articles = await QueryAsync(db => db.Articles.OrderBy(a => a.ArticleOrder).ToListAsync());
        Assert.Equal([Now, Now.AddDays(-5)], articles.Select(a => a.PublishedAt!.Value));
    }

    [Fact]
    public async Task 公開日時を指定すると講座と下書きの記事が予約公開になる()
    {
        var scheduledAt = Now.AddDays(3);
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Draft().Build();
        await SeedAsync(owner, course, new ArticleBuilder(owner).InCourse(course, 1).Draft().Build());

        var response = await CreateClientAs(owner).PutAsJsonAsync(
            $"/api/courses/{course.Id.Value}",
            new { isPublished = true, publishedAt = scheduledAt }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(scheduledAt, (await QueryAsync(db => db.Courses.SingleAsync())).PublishedAt);
        Assert.Equal(scheduledAt, (await QueryAsync(db => db.Articles.SingleAsync())).PublishedAt);
    }

    [Fact]
    public async Task 作成者以外は講座を更新できない()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var course = new CourseBuilder(owner).WithName("講座名").Build();
        await SeedAsync(owner, otherUser, course);
        var url = $"/api/courses/{course.Id.Value}";
        var payload = new { name = "書き換え" };

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().PutAsJsonAsync(url, payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClientAs(otherUser).PutAsJsonAsync(url, payload)).StatusCode);
        Assert.Equal("講座名", (await QueryAsync(db => db.Courses.SingleAsync())).Name);
    }

    #endregion

    #region DELETE /api/courses/{id}

    [Fact]
    public async Task 作成者は講座を削除でき_講座の記事も削除される()
    {
        var owner = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Build();
        var singleArticle = new ArticleBuilder(owner).WithTitle("単体記事").Build();
        await SeedAsync(owner, course, singleArticle, new ArticleBuilder(owner).InCourse(course, 1).Build());

        var response = await CreateClientAs(owner).DeleteAsync($"/api/courses/{course.Id.Value}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Courses.CountAsync()));
        Assert.Equal([singleArticle.Id], await QueryAsync(db => db.Articles.Select(a => a.Id).ToListAsync()));
    }

    [Fact]
    public async Task 作成者以外は講座を削除できない()
    {
        var owner = new UserBuilder().Build();
        var otherUser = new UserBuilder().Build();
        var course = new CourseBuilder(owner).Build();
        await SeedAsync(owner, otherUser, course);
        var url = $"/api/courses/{course.Id.Value}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClientAs(otherUser).DeleteAsync(url)).StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.Courses.CountAsync()));
    }

    [Fact]
    public async Task 存在しない講座は削除できない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).DeleteAsync($"/api/courses/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
