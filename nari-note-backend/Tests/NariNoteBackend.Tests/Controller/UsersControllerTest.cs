using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class UsersControllerTest : IntegrationTestBase
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    public UsersControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    #region GET /api/users/{id}

    [Fact]
    public async Task ユーザーのプロフィールと各種件数を取得できる()
    {
        var user = new UserBuilder().WithName("taro").WithBio("居飛車党です").WithProfileImage("https://images.test/taro").Build();
        var follower = new UserBuilder().Build();
        var following1 = new UserBuilder().Build();
        var following2 = new UserBuilder().Build();
        var published = new ArticleBuilder(user).Build();
        var othersArticle = new ArticleBuilder(follower).Build();
        await SeedAsync(
            user,
            follower,
            following1,
            following2,
            published,
            othersArticle,
            new ArticleBuilder(user).Draft().Build(),
            TestEntity.Follow(follower, user),
            TestEntity.Follow(user, following1),
            TestEntity.Follow(user, following2),
            TestEntity.Like(user, othersArticle)
        );

        var response = await CreateClient().GetAsync($"/api/users/{user.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetUserProfileResponse>(response);
        Assert.Equal(user.Id, body.Id);
        Assert.Equal("taro", body.Username);
        Assert.Equal("居飛車党です", body.Bio);
        Assert.Equal("https://images.test/taro", body.UserIconImageUrl);
        Assert.Equal(1, body.FollowerCount);
        Assert.Equal(2, body.FollowingCount);
        // 下書きは記事数に含めない
        Assert.Equal(1, body.ArticleCount);
        Assert.Equal(1, body.LikedArticleCount);
        Assert.False(body.IsFollowing);
    }

    [Fact]
    public async Task ログインユーザーがフォロー中かどうかを返す()
    {
        var user = new UserBuilder().Build();
        var follower = new UserBuilder().Build();
        var stranger = new UserBuilder().Build();
        await SeedAsync(user, follower, stranger, TestEntity.Follow(follower, user));
        var url = $"/api/users/{user.Id.Value}";

        var asFollower = await ReadAsync<GetUserProfileResponse>(await CreateClientAs(follower).GetAsync(url));
        var asStranger = await ReadAsync<GetUserProfileResponse>(await CreateClientAs(stranger).GetAsync(url));

        Assert.True(asFollower.IsFollowing);
        Assert.False(asStranger.IsFollowing);
    }

    [Fact]
    public async Task 存在しないユーザーのプロフィールは404を返す()
    {
        var response = await CreateClient().GetAsync($"/api/users/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region PUT /api/users

    [Fact]
    public async Task ユーザー名と自己紹介を更新できる()
    {
        var user = new UserBuilder().WithName("taro").WithBio("旧自己紹介").Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync("/api/users", new { name = "jiro", bio = "新自己紹介" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await QueryAsync(db => db.Users.SingleAsync());
        Assert.Equal("jiro", saved.Name);
        Assert.Equal("新自己紹介", saved.Bio);
        Assert.Equal(Now, saved.UpdatedAt);
    }

    [Fact]
    public async Task 自己紹介は空文字で消去でき_未指定の項目は変更されない()
    {
        var user = new UserBuilder().WithName("taro").WithBio("自己紹介").Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync("/api/users", new { bio = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await QueryAsync(db => db.Users.SingleAsync());
        Assert.Equal("taro", saved.Name);
        Assert.Equal("", saved.Bio);
    }

    [Fact]
    public async Task 他のユーザーが使用中の名前には変更できない()
    {
        var user = new UserBuilder().WithName("taro").Build();
        await SeedAsync(user, new UserBuilder().WithName("jiro").Build());

        var response = await CreateClientAs(user).PutAsJsonAsync("/api/users", new { name = "jiro" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("taro", (await QueryAsync(db => db.Users.SingleAsync(u => u.Id == user.Id))).Name);
    }

    [Fact]
    public async Task 自分の現在の名前はそのまま指定できる()
    {
        var user = new UserBuilder().WithName("taro").Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync("/api/users", new { name = "taro", bio = "更新" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("taro jiro")]
    [InlineData("taro@example")]
    public async Task 使用できない文字を含む名前には変更できない(string name)
    {
        var user = new UserBuilder().WithName("taro").Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync("/api/users", new { name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("taro", (await QueryAsync(db => db.Users.SingleAsync())).Name);
    }

    [Fact]
    public async Task 未認証ではプロフィールを更新できない()
    {
        var response = await CreateClient().PutAsJsonAsync("/api/users", new { name = "jiro" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region POST /api/users/icon

    static MultipartFormDataContent CreateFileContent(byte[] bytes, string contentType)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", "icon" } };
    }

    // 1x1 ピクセルの PNG 画像
    static byte[] CreatePngBytes()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
        );
    }

    [Fact(Skip = "既知の不具合 (#574): UploadUserIconService の形式検証で SKCodec がストリームを破棄し後続の Seek が失敗する。Linux では libSkiaSharp も読み込めない")]
    public async Task アイコン画像をアップロードするとプロフィール画像が更新される()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PostAsync(
            "/api/users/icon",
            CreateFileContent(CreatePngBytes(), "image/png")
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expectedUrl = Factory.ImageStorageGateway.GetUserIconUrl(user.Id.Value.ToString());
        Assert.Equal(expectedUrl, (await ReadAsync<UploadUserIconResponse>(response)).UserIconImageUrl);
        var saved = await QueryAsync(db => db.Users.SingleAsync());
        Assert.Equal(expectedUrl, saved.ProfileImage);
        Assert.Equal(Now, saved.UpdatedAt);
        Assert.Contains(user.Id.Value.ToString(), Factory.ImageStorageGateway.UploadedUserIds);
    }

    [Fact]
    public async Task 対応していない形式や画像でないファイルはアップロードできない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);
        var client = CreateClientAs(user);

        var unsupportedType = await client.PostAsync("/api/users/icon", CreateFileContent(CreatePngBytes(), "image/gif"));
        var notImage = await client.PostAsync("/api/users/icon", CreateFileContent("not an image"u8.ToArray(), "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, unsupportedType.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, notImage.StatusCode);
        Assert.Null((await QueryAsync(db => db.Users.SingleAsync())).ProfileImage);
        Assert.Empty(Factory.ImageStorageGateway.UploadedUserIds);
    }

    [Fact]
    public async Task 未認証ではアイコン画像をアップロードできない()
    {
        var response = await CreateClient().PostAsync(
            "/api/users/icon",
            CreateFileContent(CreatePngBytes(), "image/png")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region POST /api/users/{id}/follow

    [Fact]
    public async Task フォローは押すたびに付与と解除が切り替わる()
    {
        var user = new UserBuilder().Build();
        var target = new UserBuilder().Build();
        var otherFollower = new UserBuilder().Build();
        await SeedAsync(user, target, otherFollower, TestEntity.Follow(otherFollower, target));
        var client = CreateClientAs(user);
        var url = $"/api/users/{target.Id.Value}/follow";

        var followed = await ReadAsync<ToggleFollowResponse>(await client.PostAsync(url, null));
        Assert.True(followed.IsFollowing);
        Assert.Equal(2, followed.CurrentFollowerCount);

        var unfollowed = await ReadAsync<ToggleFollowResponse>(await client.PostAsync(url, null));
        Assert.False(unfollowed.IsFollowing);
        Assert.Equal(1, unfollowed.CurrentFollowerCount);
        Assert.Equal([otherFollower.Id], await QueryAsync(db => db.Follows.Select(f => f.FollowerId).ToListAsync()));
    }

    [Fact]
    public async Task 自分自身はフォローできない()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PostAsync($"/api/users/{user.Id.Value}/follow", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Follows.CountAsync()));
    }

    [Fact]
    public async Task 未認証ではフォローできない()
    {
        var target = new UserBuilder().Build();
        await SeedAsync(target);

        var response = await CreateClient().PostAsync($"/api/users/{target.Id.Value}/follow", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/users/{id}/followers, followings

    [Fact]
    public async Task フォロワーとフォロー中のユーザーを取得できる()
    {
        var user = new UserBuilder().WithName("taro").Build();
        var follower = new UserBuilder().WithName("follower").WithProfileImage("https://images.test/follower").Build();
        var following = new UserBuilder().WithName("following").Build();
        await SeedAsync(
            user,
            follower,
            following,
            TestEntity.Follow(follower, user),
            TestEntity.Follow(user, following)
        );
        var client = CreateClient();

        var followers = await ReadAsync<GetFollowersResponse>(
            await client.GetAsync($"/api/users/{user.Id.Value}/followers")
        );
        var followings = await ReadAsync<GetFollowingsResponse>(
            await client.GetAsync($"/api/users/{user.Id.Value}/followings")
        );

        var followerDto = Assert.Single(followers.Followers);
        Assert.Equal(follower.Id, followerDto.Id);
        Assert.Equal("follower", followerDto.Username);
        Assert.Equal("https://images.test/follower", followerDto.UserIconImageUrl);
        Assert.Equal(["following"], followings.Followings.Select(f => f.Username));
    }

    [Theory]
    [InlineData("followers")]
    [InlineData("followings")]
    public async Task 存在しないユーザーのフォロー関係は404を返す(string path)
    {
        var response = await CreateClient().GetAsync($"/api/users/{Guid.CreateVersion7()}/{path}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GET /api/users/{id}/liked-articles

    [Fact]
    public async Task いいねした記事を新しくいいねした順に取得できる()
    {
        var user = new UserBuilder().Build();
        var author = new UserBuilder().WithName("author").Build();
        var first = new ArticleBuilder(author).WithTitle("先にいいねした記事").Build();
        var second = new ArticleBuilder(author).WithTitle("後でいいねした記事").Build();
        var notLiked = new ArticleBuilder(author).WithTitle("いいねしていない記事").Build();
        var tag = TestEntity.Tag("振り飛車");
        await SeedAsync(
            user,
            author,
            first,
            second,
            notLiked,
            tag,
            TestEntity.ArticleTag(second, tag),
            TestEntity.Like(user, first, Now.AddDays(-2)),
            TestEntity.Like(user, second, Now.AddDays(-1)),
            TestEntity.Like(author, second, Now.AddDays(-1))
        );

        var response = await CreateClient().GetAsync($"/api/users/{user.Id.Value}/liked-articles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetLikedArticlesResponse>(response);
        Assert.Equal(user.Id, body.UserId);
        Assert.Equal(["後でいいねした記事", "先にいいねした記事"], body.Articles.Select(a => a.Title));
        Assert.Equal("author", body.Articles[0].AuthorName);
        Assert.Equal(["振り飛車"], body.Articles[0].Tags);
        Assert.Equal(2, body.Articles[0].LikeCount);
    }

    #endregion
}
