using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class AuthControllerTest : IntegrationTestBase
{
    const string Password = "password123";

    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    public AuthControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    #region POST /api/auth/signup

    [Fact]
    public async Task サインアップすると未認証ユーザーと確認トークンが作成される()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/signup", new
        {
            name = "taro",
            email = "Taro@Example.com",
            password = Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await QueryAsync(db => db.Users.SingleAsync());
        Assert.Equal("taro", user.Name);
        Assert.Equal("taro@example.com", user.Email);
        Assert.False(user.IsEmailVerified);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));

        var verification = await QueryAsync(db => db.EmailVerifications.SingleAsync());
        Assert.Equal(user.Id, verification.UserId);
        Assert.Equal(Now.AddHours(24), verification.ExpiresAt);
        Assert.False(verification.IsUsed);
    }

    [Fact]
    public async Task サインアップすると確認メールとDiscord通知がOutbox経由で送信される()
    {
        await CreateClient().PostAsJsonAsync("/api/auth/signup", new
        {
            name = "taro",
            email = "taro@example.com",
            password = Password
        });

        var messages = await QueryAsync(db => db.OutboxMessages.ToListAsync());
        Assert.Equal(
            [OutboxMessage.DiscordEmbedType, OutboxMessage.EmailType],
            messages.Select(m => m.Type).Order()
        );
        Assert.All(messages, m => Assert.Equal(Now, m.NextAttemptAt));

        // リクエスト中には送信せず、Outbox の処理で送信する
        Assert.Empty(Factory.EmailHelper.SentMessages);

        await ProcessOutboxAsync();

        var email = Assert.Single(Factory.EmailHelper.SentMessages);
        Assert.Equal(["taro@example.com"], email.To);
        Assert.Contains("メールアドレスの確認", email.Subject);
        Assert.Single(Factory.DiscordNotifier.Embeds);
        Assert.All(
            await QueryAsync(db => db.OutboxMessages.ToListAsync()),
            m => Assert.Equal(Now, m.ProcessedAt)
        );
    }

    [Fact]
    public async Task 登録済みのメールアドレスでも同じレスポンスを返しユーザーは作成しない()
    {
        var existing = new UserBuilder().WithEmail("taro@example.com").Build();
        await SeedAsync(existing);

        var response = await CreateClient().PostAsJsonAsync("/api/auth/signup", new
        {
            name = "another",
            email = "taro@example.com",
            password = Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.Users.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.EmailVerifications.CountAsync()));

        // 登録済みである旨の案内メールだけを送る
        await ProcessOutboxAsync();
        var email = Assert.Single(Factory.EmailHelper.SentMessages);
        Assert.Equal(["taro@example.com"], email.To);
        Assert.Empty(Factory.DiscordNotifier.Embeds);
    }

    [Fact]
    public async Task 未認証のメールアドレスで再度サインアップすると確認メールを再送する()
    {
        var existing = new UserBuilder().WithName("taro").WithEmail("taro@example.com").EmailUnverified().Build();
        await SeedAsync(existing);

        var response = await CreateClient().PostAsJsonAsync("/api/auth/signup", new
        {
            name = "changed",
            email = "taro@example.com",
            password = "another-password"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await QueryAsync(db => db.Users.SingleAsync());
        Assert.Equal("taro", user.Name);
        Assert.Equal(existing.PasswordHash, user.PasswordHash);
        Assert.Equal(1, await QueryAsync(db => db.EmailVerifications.CountAsync()));
    }

    [Fact]
    public async Task 使用済みのユーザー名ではサインアップできない()
    {
        await SeedAsync(new UserBuilder().WithName("taro").Build());

        var response = await CreateClient().PostAsJsonAsync("/api/auth/signup", new
        {
            name = "taro",
            email = "new@example.com",
            password = Password
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.Users.CountAsync()));
    }

    [Theory]
    [InlineData("taro", "taro@example.com", "short")]
    [InlineData("taro", "not-an-email", Password)]
    [InlineData("", "taro@example.com", Password)]
    public async Task 入力が不正な場合はサインアップできない(string name, string email, string password)
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/signup", new { name, email, password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Users.CountAsync()));
    }

    #endregion

    #region POST /api/auth/signin

    [Theory]
    [InlineData("taro")]
    [InlineData("taro@example.com")]
    [InlineData("TARO@example.com")]
    public async Task ユーザー名またはメールアドレスでサインインできる(string usernameOrEmail)
    {
        var user = new UserBuilder().WithName("taro").WithEmail("taro@example.com").WithPassword(Password).Build();
        await SeedAsync(user);

        var response = await CreateClient().PostAsJsonAsync(
            "/api/auth/signin",
            new { usernameOrEmail, password = Password }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(user.Id, (await ReadAsync<AuthResponse>(response)).UserId);

        // 発行された Cookie で認証できる
        var token = GetAuthToken(response);
        Assert.False(string.IsNullOrEmpty(token));
        var me = await ReadAsync<AuthResponse>(await CreateClientWithToken(token!).GetAsync("/api/auth/me"));
        Assert.Equal(user.Id, me.UserId);
    }

    [Theory]
    [InlineData("taro", "wrong-password")]
    [InlineData("unknown", Password)]
    public async Task ユーザー名かパスワードが誤っているとサインインできない(string usernameOrEmail, string password)
    {
        await SeedAsync(new UserBuilder().WithName("taro").WithPassword(Password).Build());

        var response = await CreateClient().PostAsJsonAsync("/api/auth/signin", new { usernameOrEmail, password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(GetAuthToken(response));
    }

    #endregion

    #region GET /api/auth/me

    [Fact]
    public async Task 未認証で現在のユーザーを取得すると空の情報を返す()
    {
        var response = await CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<AuthResponse>(response);
        Assert.Null(body.UserId);
        Assert.Null(body.UserName);
        Assert.Null(body.UserIconImageUrl);
    }

    [Fact]
    public async Task 認証済みなら現在のユーザー情報を返す()
    {
        var user = new UserBuilder().WithName("taro").Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).GetAsync("/api/auth/me");

        var body = await ReadAsync<AuthResponse>(response);
        Assert.Equal(user.Id, body.UserId);
        Assert.Equal("taro", body.UserName);
        Assert.Equal(Factory.ImageStorageGateway.GetUserIconUrl(user.Id.Value.ToString()), body.UserIconImageUrl);
    }

    [Fact]
    public async Task 認証トークンは有効期限を過ぎると無効になる()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);
        var client = CreateClientAs(user);
        var expiresAt = Now.AddHours(168);

        TimeProvider.SetUtcNow(expiresAt);
        Assert.Equal(user.Id, (await ReadAsync<AuthResponse>(await client.GetAsync("/api/auth/me"))).UserId);

        TimeProvider.SetUtcNow(expiresAt.AddSeconds(1));
        Assert.Null((await ReadAsync<AuthResponse>(await client.GetAsync("/api/auth/me"))).UserId);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/articles/my")).StatusCode);
    }

    [Fact]
    public async Task 不正な認証トークンは未認証として扱う()
    {
        var client = CreateClientWithToken("invalid-token");

        Assert.Null((await ReadAsync<AuthResponse>(await client.GetAsync("/api/auth/me"))).UserId);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/articles/my")).StatusCode);
    }

    [Fact]
    public async Task 存在しないユーザーの認証トークンは未認証として扱う()
    {
        // DB に保存していない（退会済み相当の）ユーザーのトークン
        var client = CreateClientAs(new UserBuilder().Build());

        Assert.Null((await ReadAsync<AuthResponse>(await client.GetAsync("/api/auth/me"))).UserId);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/articles/my")).StatusCode);
    }

    #endregion

    #region POST /api/auth/verify-email

    [Fact]
    public async Task 確認トークンでメールアドレスを認証するとサインイン状態になる()
    {
        var user = new UserBuilder().EmailUnverified().Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "valid-token", Now.AddHours(1)));

        var response = await CreateClient().PostAsJsonAsync("/api/auth/verify-email", new { token = "valid-token" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(user.Id, (await ReadAsync<AuthResponse>(response)).UserId);
        Assert.False(string.IsNullOrEmpty(GetAuthToken(response)));
        Assert.True((await QueryAsync(db => db.Users.SingleAsync())).IsEmailVerified);
        Assert.True((await QueryAsync(db => db.EmailVerifications.SingleAsync())).IsUsed);
    }

    [Fact]
    public async Task 確認トークンは有効期限ちょうどまで使用できる()
    {
        var user = new UserBuilder().EmailUnverified().Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "valid-token", Now));

        var response = await CreateClient().PostAsJsonAsync("/api/auth/verify-email", new { token = "valid-token" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task 期限切れの確認トークンでは認証できない()
    {
        var user = new UserBuilder().EmailUnverified().Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "expired-token", Now.AddSeconds(-1)));

        var response = await CreateClient().PostAsJsonAsync("/api/auth/verify-email", new { token = "expired-token" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False((await QueryAsync(db => db.Users.SingleAsync())).IsEmailVerified);
    }

    [Fact]
    public async Task 使用済みまたは存在しない確認トークンでは認証できない()
    {
        var user = new UserBuilder().EmailUnverified().Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "used-token", Now.AddHours(1), true));
        var client = CreateClient();

        var used = await client.PostAsJsonAsync("/api/auth/verify-email", new { token = "used-token" });
        var unknown = await client.PostAsJsonAsync("/api/auth/verify-email", new { token = "unknown-token" });

        Assert.Equal(HttpStatusCode.BadRequest, used.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.False((await QueryAsync(db => db.Users.SingleAsync())).IsEmailVerified);
    }

    #endregion

    #region PUT /api/auth/password

    [Fact]
    public async Task 現在のパスワードが正しければパスワードを変更できる()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync(
            "/api/auth/password",
            new { currentPassword = Password, newPassword = "new-password123" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await QueryAsync(db => db.Users.SingleAsync());
        Assert.True(BCrypt.Net.BCrypt.Verify("new-password123", saved.PasswordHash));
        Assert.Equal(Now, saved.UpdatedAt);
    }

    [Fact]
    public async Task 現在のパスワードが誤っているとパスワードを変更できない()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PutAsJsonAsync(
            "/api/auth/password",
            new { currentPassword = "wrong-password", newPassword = "new-password123" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(user.PasswordHash, (await QueryAsync(db => db.Users.SingleAsync())).PasswordHash);
    }

    [Fact]
    public async Task 未認証ではパスワードを変更できない()
    {
        var response = await CreateClient().PutAsJsonAsync(
            "/api/auth/password",
            new { currentPassword = Password, newPassword = "new-password123" }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region POST /api/auth/forgot-password

    [Fact]
    public async Task パスワード再設定を申請すると再設定トークンとメールが作成される()
    {
        var user = new UserBuilder().WithEmail("taro@example.com").Build();
        await SeedAsync(user);

        var response = await CreateClient().PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = "taro@example.com" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await QueryAsync(db => db.PasswordResetTokens.SingleAsync());
        Assert.Equal(user.Id, token.UserId);
        Assert.Equal(Now.AddHours(1), token.ExpiresAt);
        Assert.False(token.IsUsed);

        await ProcessOutboxAsync();
        var email = Assert.Single(Factory.EmailHelper.SentMessages);
        Assert.Equal(["taro@example.com"], email.To);
        Assert.Contains(token.Token, email.TextBody);
    }

    [Fact]
    public async Task 未登録のメールアドレスでも同じレスポンスを返しトークンは作成しない()
    {
        var response = await CreateClient().PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = "unknown@example.com" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.PasswordResetTokens.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.OutboxMessages.CountAsync()));
    }

    #endregion

    #region POST /api/auth/reset-password

    [Fact]
    public async Task 再設定トークンでパスワードを再設定できる()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        // 有効期限ちょうどまで使用できる
        await SeedAsync(user, TestEntity.PasswordResetToken(user, "valid-token", Now));

        var response = await CreateClient().PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token = "valid-token", newPassword = "new-password123" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await QueryAsync(db => db.Users.SingleAsync());
        Assert.True(BCrypt.Net.BCrypt.Verify("new-password123", saved.PasswordHash));
        Assert.Equal(Now, saved.UpdatedAt);
        Assert.True((await QueryAsync(db => db.PasswordResetTokens.SingleAsync())).IsUsed);
    }

    [Theory]
    [InlineData("expired-token")]
    [InlineData("used-token")]
    [InlineData("unknown-token")]
    public async Task 期限切れ_使用済み_存在しない再設定トークンでは再設定できない(string token)
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        await SeedAsync(
            user,
            TestEntity.PasswordResetToken(user, "expired-token", Now.AddSeconds(-1)),
            TestEntity.PasswordResetToken(user, "used-token", Now.AddHours(1), true)
        );

        var response = await CreateClient().PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token, newPassword = "new-password123" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(user.PasswordHash, (await QueryAsync(db => db.Users.SingleAsync())).PasswordHash);
    }

    #endregion

    #region POST /api/auth/logout

    [Fact]
    public async Task ログアウトすると認証Cookieを削除する()
    {
        var user = new UserBuilder().Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("", GetAuthToken(response));
    }

    #endregion

    #region POST /api/auth/withdraw

    [Fact]
    public async Task 退会するとユーザーと関連データが削除される()
    {
        var user = new UserBuilder().WithPassword(Password).WithProfileImage("https://images.test/icon").Build();
        var otherUser = new UserBuilder().Build();
        var otherArticle = new ArticleBuilder(otherUser).Build();
        await SeedAsync(
            user,
            otherUser,
            otherArticle,
            new ArticleBuilder(user).Build(),
            new CourseBuilder(user).Build(),
            TestEntity.Like(user, otherArticle),
            TestEntity.Comment(user, otherArticle, "コメント"),
            TestEntity.Follow(user, otherUser)
        );
        Factory.ImageStorageGateway.UploadedUserIds.Add(user.Id.Value.ToString());

        var response = await CreateClientAs(user).PostAsJsonAsync("/api/auth/withdraw", new { password = Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("", GetAuthToken(response));
        Assert.Equal([otherUser.Id], await QueryAsync(db => db.Users.Select(u => u.Id).ToListAsync()));
        Assert.Equal([otherArticle.Id], await QueryAsync(db => db.Articles.Select(a => a.Id).ToListAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Courses.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Likes.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Comments.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.Follows.CountAsync()));
        Assert.Empty(Factory.ImageStorageGateway.UploadedUserIds);
    }

    [Fact]
    public async Task パスワードが誤っていると退会できない()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        await SeedAsync(user);

        var response = await CreateClientAs(user).PostAsJsonAsync(
            "/api/auth/withdraw",
            new { password = "wrong-password" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.Users.CountAsync()));
    }

    #endregion
}
