using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class VerifyEmailServiceTest
{
    const string Token = "verify-token";

    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly ICookieOptionsHelper cookieOptionsHelper = Substitute.For<ICookieOptionsHelper>();
    readonly IEmailVerificationRepository emailVerificationRepository = Substitute.For<IEmailVerificationRepository>();
    readonly HttpResponse httpResponse = new DefaultHttpContext().Response;
    readonly IJwtHelper jwtHelper = Substitute.For<IJwtHelper>();
    readonly VerifyEmailService service;
    readonly User user = new UserBuilder().EmailUnverified().Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public VerifyEmailServiceTest()
    {
        this.jwtHelper.GenerateToken(this.user).Returns("jwt-token");
        this.cookieOptionsHelper
            .CreateAuthCookieOptions(Arg.Any<TimeSpan>())
            .Returns(new CookieOptions { HttpOnly = true, Path = "/" });

        this.service = new VerifyEmailService(
            this.userRepository,
            this.emailVerificationRepository,
            this.jwtHelper,
            this.cookieOptionsHelper,
            new TestTimeProvider()
        );
    }

    string SetCookieHeader => this.httpResponse.Headers.SetCookie.ToString();

    EmailVerification SetupVerification(DateTime expiresAt, bool isUsed = false)
    {
        var verification = TestEntity.EmailVerification(this.user, Token, expiresAt, isUsed);
        verification.User = this.user;
        this.emailVerificationRepository.FindByTokenAsync(Token).Returns(verification);
        return verification;
    }

    Task Execute()
    {
        return this.service.ExecuteAsync(new VerifyEmailRequest { Token = Token }, this.httpResponse);
    }

    [Fact]
    public async Task 有効なトークンならメールアドレスを認証済みにしてサインイン状態にする()
    {
        var verification = SetupVerification(Now.AddHours(1));

        var response = await this.service.ExecuteAsync(new VerifyEmailRequest { Token = Token }, this.httpResponse);

        Assert.Equal(this.user.Id, response.UserId);
        Assert.True(verification.IsUsed);
        await this.emailVerificationRepository.Received(1).UpdateAsync(verification);
        Assert.True(this.user.IsEmailVerified);
        await this.userRepository.Received(1).UpdateAsync(this.user);
        Assert.Contains("authToken=jwt-token", SetCookieHeader);
    }

    [Fact]
    public async Task パスワードが置き換わる場合は以前に発行された認証トークンを無効にする()
    {
        var verification = SetupVerification(Now.AddHours(1));
        verification.PasswordHash = "new-password-hash";
        this.jwtHelper
            .GenerateToken(this.user)
            .Returns(_ => $"jwt-{this.user.SecurityStamp}");

        await Execute();

        // 認証前に別の人がサインインしていても、そのトークンは使えなくなる
        Assert.NotNull(this.user.SecurityStamp);
        // 新しく発行するトークンは更新後の SecurityStamp で作る
        Assert.Contains($"authToken=jwt-{this.user.SecurityStamp}", SetCookieHeader);
    }

    [Fact]
    public async Task パスワードが変わらない場合は発行済みの認証トークンを無効にしない()
    {
        // 別の端末で確認メールを開いても、サインイン済みの端末はそのまま使えるようにする
        var verification = SetupVerification(Now.AddHours(1));
        verification.PasswordHash = this.user.PasswordHash;

        await Execute();

        Assert.Null(this.user.SecurityStamp);
    }

    [Fact]
    public async Task トークンに保存されたユーザー名とパスワードを反映する()
    {
        var verification = SetupVerification(Now.AddHours(1));
        verification.Name = "new-name";
        verification.PasswordHash = "new-password-hash";

        await Execute();

        Assert.Equal("new-name", this.user.Name);
        Assert.Equal("new-password-hash", this.user.PasswordHash);
    }

    [Fact]
    public async Task トークンのユーザー名が他のユーザーに使われていれば現在の名前のままにする()
    {
        var currentName = this.user.Name;
        var verification = SetupVerification(Now.AddHours(1));
        verification.Name = "taken-name";
        verification.PasswordHash = "new-password-hash";
        this.userRepository.ExistsByNameAsync("taken-name", this.user.Id).Returns(true);

        await Execute();

        Assert.Equal(currentName, this.user.Name);
        Assert.Equal("new-password-hash", this.user.PasswordHash);
    }

    [Fact]
    public async Task トークンにユーザー名とパスワードが無ければ現在の値のままにする()
    {
        // 確認メールの再送で発行したトークン
        var currentName = this.user.Name;
        var currentPasswordHash = this.user.PasswordHash;
        SetupVerification(Now.AddHours(1));

        await Execute();

        Assert.Equal(currentName, this.user.Name);
        Assert.Equal(currentPasswordHash, this.user.PasswordHash);
    }

    [Fact]
    public async Task 有効期限ちょうどのトークンは使用できる()
    {
        SetupVerification(Now);

        await Execute();

        Assert.True(this.user.IsEmailVerified);
    }

    [Fact]
    public async Task 有効期限を過ぎたトークンは使用できない()
    {
        SetupVerification(Now.AddTicks(-1));

        await Assert.ThrowsAsync<ArgumentException>(Execute);

        Assert.False(this.user.IsEmailVerified);
        Assert.Equal("", SetCookieHeader);
    }

    [Fact]
    public async Task 使用済みのトークンは使用できない()
    {
        SetupVerification(Now.AddHours(1), true);

        await Assert.ThrowsAsync<ArgumentException>(Execute);

        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
        Assert.Equal("", SetCookieHeader);
    }

    [Fact]
    public async Task 存在しないトークンは使用できない()
    {
        await Assert.ThrowsAsync<ArgumentException>(Execute);

        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
        Assert.Equal("", SetCookieHeader);
    }
}
