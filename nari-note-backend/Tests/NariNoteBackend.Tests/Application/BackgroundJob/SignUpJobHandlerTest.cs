using Moq;
using NariNoteBackend.Application.BackgroundJob;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Application.BackgroundJob;

public class SignUpJobHandlerTest
{
    const string Email = "taro@example.com";

    readonly Mock<IUserRepository> userRepository = new();
    readonly Mock<IEmailVerificationRepository> emailVerificationRepository = new();
    readonly Mock<IEmailHelper> emailHelper = new();
    readonly Mock<IDiscordNotifier> discordNotifier = new();
    readonly List<EmailMessage> sentMessages = new();
    readonly List<EmailVerification> createdVerifications = new();

    public SignUpJobHandlerTest()
    {
        this.emailHelper
            .Setup(h => h.SendAsync(It.IsAny<EmailMessage>()))
            .Callback<EmailMessage>(m => this.sentMessages.Add(m))
            .Returns(Task.CompletedTask);
        this.emailVerificationRepository
            .Setup(r => r.CreateAsync(It.IsAny<EmailVerification>()))
            .Callback<EmailVerification>(v => this.createdVerifications.Add(v))
            .ReturnsAsync((EmailVerification v) => v);
        this.userRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = UserId.From(Guid.NewGuid());
                return u;
            });
    }

    SignUpJobHandler CreateHandler()
    {
        return new SignUpJobHandler(
            this.userRepository.Object,
            this.emailVerificationRepository.Object,
            this.emailHelper.Object,
            this.discordNotifier.Object
        );
    }

    static SignUpJob CreateJob()
    {
        return new SignUpJob("taro", Email, BCrypt.Net.BCrypt.HashPassword("password123"));
    }

    void SetupExistingUser(User? user)
    {
        this.userRepository.Setup(r => r.FindByEmailAsync(Email)).ReturnsAsync(user);
    }

    static User CreateExistingUser(bool isEmailVerified)
    {
        return new User
        {
            Id = UserId.From(Guid.NewGuid()),
            Name = "existing",
            Email = Email,
            PasswordHash = "existing-hash",
            IsEmailVerified = isEmailVerified
        };
    }

    [Fact]
    public async Task 未登録のメールアドレスならユーザーを作成して確認メールを送る()
    {
        SetupExistingUser(null);
        var job = CreateJob();

        await CreateHandler().ExecuteAsync(job);

        this.userRepository.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Name == job.Name && u.Email == job.Email && u.PasswordHash == job.PasswordHash
        )), Times.Once);

        var verification = Assert.Single(this.createdVerifications);
        var message = Assert.Single(this.sentMessages);
        Assert.Equal([Email], message.To);
        Assert.Equal(EmailMessageStore.SignupMessage(Email, Guid.Parse(verification.Token)).Subject, message.Subject);
        Assert.Contains(verification.Token, message.TextBody);

        this.discordNotifier.Verify(n => n.NotifyWithEmbedAsync(It.IsAny<DiscordEmbed>()), Times.Once);
    }

    [Fact]
    public async Task 認証済みのメールアドレスなら登録済み通知メールのみを送る()
    {
        SetupExistingUser(CreateExistingUser(isEmailVerified: true));

        await CreateHandler().ExecuteAsync(CreateJob());

        this.userRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        this.userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        Assert.Empty(this.createdVerifications);

        var message = Assert.Single(this.sentMessages);
        Assert.Equal([Email], message.To);
        Assert.Equal(EmailMessageStore.AlreadyRegisteredMessage(Email).Subject, message.Subject);
        Assert.Contains("https://nari-note.com/forgot-password", message.TextBody);

        this.discordNotifier.Verify(n => n.NotifyWithEmbedAsync(It.IsAny<DiscordEmbed>()), Times.Never);
    }

    [Fact]
    public async Task 未認証のメールアドレスなら既存アカウントを変更せず確認メールを再送する()
    {
        var existingUser = CreateExistingUser(isEmailVerified: false);
        SetupExistingUser(existingUser);

        await CreateHandler().ExecuteAsync(CreateJob());

        this.userRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        this.userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        Assert.Equal("existing", existingUser.Name);
        Assert.Equal("existing-hash", existingUser.PasswordHash);

        var verification = Assert.Single(this.createdVerifications);
        Assert.Equal(existingUser.Id, verification.UserId);

        var message = Assert.Single(this.sentMessages);
        Assert.Equal([Email], message.To);
        Assert.Contains(verification.Token, message.TextBody);

        this.discordNotifier.Verify(n => n.NotifyWithEmbedAsync(It.IsAny<DiscordEmbed>()), Times.Never);
    }
}
