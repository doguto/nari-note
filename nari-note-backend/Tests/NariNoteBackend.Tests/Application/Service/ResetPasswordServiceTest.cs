using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class ResetPasswordServiceTest
{
    const string Token = "reset-token";

    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly IPasswordResetTokenRepository passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
    readonly ResetPasswordService service;
    readonly User user = new UserBuilder().Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public ResetPasswordServiceTest()
    {
        this.service = new ResetPasswordService(
            this.userRepository,
            this.passwordResetTokenRepository,
            new TestTimeProvider()
        );
    }

    PasswordResetToken SetupToken(DateTime expiresAt, bool isUsed = false)
    {
        var token = TestEntity.PasswordResetToken(this.user, Token, expiresAt, isUsed);
        token.User = this.user;
        this.passwordResetTokenRepository.FindByTokenAsync(Token).Returns(token);
        return token;
    }

    static ResetPasswordRequest CreateRequest()
    {
        return new ResetPasswordRequest { Token = Token, NewPassword = "new-password123" };
    }

    [Fact]
    public async Task 有効なトークンならパスワードを更新しトークンを使用済みにする()
    {
        var token = SetupToken(Now.AddMinutes(30));

        await this.service.ExecuteAsync(CreateRequest());

        Assert.True(token.IsUsed);
        await this.passwordResetTokenRepository.Received(1).UpdateAsync(token);
        Assert.True(BCrypt.Net.BCrypt.Verify("new-password123", this.user.PasswordHash));
        Assert.Equal(Now, this.user.UpdatedAt);
        await this.userRepository.Received(1).UpdateAsync(this.user);
    }

    [Fact]
    public async Task 有効期限ちょうどのトークンは使用できる()
    {
        SetupToken(Now);

        await this.service.ExecuteAsync(CreateRequest());

        await this.userRepository.Received(1).UpdateAsync(this.user);
    }

    [Fact]
    public async Task 有効期限を過ぎたトークンは使用できない()
    {
        SetupToken(Now.AddTicks(-1));

        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(CreateRequest()));

        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task 使用済みのトークンは使用できない()
    {
        SetupToken(Now.AddMinutes(30), true);

        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(CreateRequest()));

        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task 存在しないトークンは使用できない()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(CreateRequest()));

        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }
}
