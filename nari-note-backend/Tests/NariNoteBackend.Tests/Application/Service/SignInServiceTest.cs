using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class SignInServiceTest
{
    const string Password = "password123";

    readonly ICookieOptionsHelper cookieOptionsHelper = Substitute.For<ICookieOptionsHelper>();
    readonly HttpResponse httpResponse = new DefaultHttpContext().Response;
    readonly IJwtHelper jwtHelper = Substitute.For<IJwtHelper>();
    readonly SignInService service;
    readonly User user = new UserBuilder().WithName("taro").WithPassword(Password).Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public SignInServiceTest()
    {
        this.userRepository.FindByUsernameOrEmailAsync("taro").Returns(this.user);
        this.jwtHelper.GenerateToken(this.user.Id, "taro").Returns("jwt-token");
        this.jwtHelper.GetExpiration().Returns(24);
        this.cookieOptionsHelper
            .CreateAuthCookieOptions(Arg.Any<TimeSpan>())
            .Returns(new CookieOptions { HttpOnly = true, Path = "/" });

        this.service = new SignInService(this.userRepository, this.jwtHelper, this.cookieOptionsHelper);
    }

    string SetCookieHeader => this.httpResponse.Headers.SetCookie.ToString();

    [Fact]
    public async Task パスワードが正しければ認証トークンをCookieに設定する()
    {
        var response = await this.service.ExecuteAsync(
            new SignInRequest { UsernameOrEmail = "taro", Password = Password },
            this.httpResponse
        );

        Assert.Equal(this.user.Id, response.UserId);
        Assert.Contains("authToken=jwt-token", SetCookieHeader);
        // Cookie の有効期間はトークンの有効期間に合わせる
        this.cookieOptionsHelper.Received(1).CreateAuthCookieOptions(TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task パスワードが誤っていればトークンを発行しない()
    {
        var request = new SignInRequest { UsernameOrEmail = "taro", Password = "wrong-password" };

        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(request, this.httpResponse));

        Assert.Equal("", SetCookieHeader);
        this.jwtHelper.DidNotReceiveWithAnyArgs().GenerateToken(this.user.Id, "");
    }

    [Fact]
    public async Task ユーザーが存在しない場合もパスワード誤りと同じエラーにする()
    {
        // ユーザーの存在有無をエラーメッセージから判別できないようにするため
        var wrongPassword = await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteAsync(
                new SignInRequest { UsernameOrEmail = "taro", Password = "wrong-password" },
                this.httpResponse
            )
        );
        var unknownUser = await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteAsync(
                new SignInRequest { UsernameOrEmail = "unknown", Password = Password },
                this.httpResponse
            )
        );

        Assert.Equal(wrongPassword.Message, unknownUser.Message);
        Assert.Equal("", SetCookieHeader);
    }
}
