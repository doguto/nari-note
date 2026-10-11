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

public class SignUpServiceTest
{
    const string Email = "taro@example.com";

    readonly List<User> createdUsers = new();
    readonly List<EmailVerification> createdVerifications = new();
    readonly IEmailVerificationRepository emailVerificationRepository = Substitute.For<IEmailVerificationRepository>();
    readonly List<OutboxMessage> outboxMessages = new();
    readonly IOutboxMessageRepository outboxMessageRepository = Substitute.For<IOutboxMessageRepository>();
    readonly SignUpService service;
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public SignUpServiceTest()
    {
        this.userRepository
            .CreateAsync(Arg.Any<User>())
            .Returns(call =>
            {
                var user = call.Arg<User>();
                user.Id = UserId.From(Guid.CreateVersion7());
                this.createdUsers.Add(user);
                return user;
            });
        this.emailVerificationRepository
            .CreateAsync(Arg.Do<EmailVerification>(this.createdVerifications.Add))
            .Returns(call => call.Arg<EmailVerification>());
        this.outboxMessageRepository
            .AddAsync(Arg.Do<OutboxMessage>(this.outboxMessages.Add))
            .Returns(Task.CompletedTask);

        this.service = new SignUpService(
            this.userRepository,
            this.emailVerificationRepository,
            this.outboxMessageRepository,
            new TestTimeProvider()
        );
    }

    static SignUpRequest CreateRequest(string email = Email)
    {
        return new SignUpRequest { Name = "taro", Email = email, Password = "password123" };
    }

    [Fact]
    public async Task 未登録のメールアドレスならユーザーと確認トークンを作成する()
    {
        await this.service.ExecuteAsync(CreateRequest(" Taro@Example.com "));

        var user = Assert.Single(this.createdUsers);
        Assert.Equal("taro", user.Name);
        Assert.Equal(Email, user.Email);
        Assert.False(user.IsEmailVerified);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
        var verification = Assert.Single(this.createdVerifications);
        Assert.Equal(user.Id, verification.UserId);
        Assert.Equal(TestTimeProvider.DefaultUtcNow.AddHours(24), verification.ExpiresAt);
        Assert.True(Guid.TryParse(verification.Token, out _));
    }

    [Fact]
    public async Task 未登録のメールアドレスなら確認メールとDiscord通知をOutboxに積む()
    {
        await this.service.ExecuteAsync(CreateRequest());

        Assert.Equal(
            [OutboxMessage.EmailType, OutboxMessage.DiscordEmbedType],
            this.outboxMessages.Select(m => m.Type)
        );
        Assert.All(this.outboxMessages, m => Assert.Equal(TestTimeProvider.DefaultUtcNow, m.NextAttemptAt));
        // 確認メールには発行したトークンが含まれる
        Assert.Contains(this.createdVerifications[0].Token, this.outboxMessages[0].Payload);
    }

    [Fact]
    public async Task 認証済みのメールアドレスなら登録済みの案内メールだけを積む()
    {
        this.userRepository.FindByEmailAsync(Email).Returns(new UserBuilder().WithEmail(Email).Build());

        await this.service.ExecuteAsync(CreateRequest());

        await this.userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        Assert.Empty(this.createdVerifications);
        Assert.Equal([OutboxMessage.EmailType], this.outboxMessages.Select(m => m.Type));
    }

    [Fact]
    public async Task 未認証のメールアドレスなら既存ユーザーに確認メールを再送する()
    {
        var existing = new UserBuilder().WithEmail(Email).EmailUnverified().Build();
        this.userRepository.FindByEmailAsync(Email).Returns(existing);

        await this.service.ExecuteAsync(CreateRequest());

        await this.userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        Assert.Equal(existing.Id, Assert.Single(this.createdVerifications).UserId);
        Assert.Equal([OutboxMessage.EmailType], this.outboxMessages.Select(m => m.Type));
    }

    [Fact]
    public async Task ユーザー名が使用済みなら作成しない()
    {
        this.userRepository.ExistsByNameAsync("taro").Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => this.service.ExecuteAsync(CreateRequest()));

        await this.userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        Assert.Empty(this.outboxMessages);
    }

    [Fact]
    public async Task 登録済みのメールアドレスならユーザー名が重複していてもエラーにしない()
    {
        // メールアドレスの登録有無を外部から判別できないようにするため
        this.userRepository.FindByEmailAsync(Email).Returns(new UserBuilder().WithEmail(Email).Build());
        this.userRepository.ExistsByNameAsync("taro").Returns(true);

        await this.service.ExecuteAsync(CreateRequest());

        Assert.Single(this.outboxMessages);
    }
}
