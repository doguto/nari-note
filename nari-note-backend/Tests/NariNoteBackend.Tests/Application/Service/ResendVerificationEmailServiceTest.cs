using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class ResendVerificationEmailServiceTest
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly List<EmailVerification> createdVerifications = new();
    readonly IEmailVerificationRepository emailVerificationRepository = Substitute.For<IEmailVerificationRepository>();
    readonly List<OutboxMessage> outboxMessages = new();
    readonly IOutboxMessageRepository outboxMessageRepository = Substitute.For<IOutboxMessageRepository>();
    readonly ResendVerificationEmailService service;
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public ResendVerificationEmailServiceTest()
    {
        this.emailVerificationRepository
            .CreateAsync(Arg.Do<EmailVerification>(this.createdVerifications.Add))
            .Returns(call => call.Arg<EmailVerification>());
        this.outboxMessageRepository
            .AddAsync(Arg.Do<OutboxMessage>(this.outboxMessages.Add))
            .Returns(Task.CompletedTask);

        this.service = new ResendVerificationEmailService(
            this.userRepository,
            this.emailVerificationRepository,
            this.outboxMessageRepository,
            new TestTimeProvider()
        );
    }

    User SetupUser(bool isEmailVerified = false)
    {
        var builder = new UserBuilder();
        var user = isEmailVerified ? builder.Build() : builder.EmailUnverified().Build();
        this.userRepository.FindForceByIdAsync(user.Id).Returns(user);
        return user;
    }

    void SetupLatestVerification(User user, DateTime createdAt)
    {
        var verification = TestEntity.EmailVerification(user, "previous-token", createdAt.AddHours(24));
        verification.CreatedAt = createdAt;
        this.emailVerificationRepository.FindLatestByUserIdAsync(user.Id).Returns(verification);
    }

    [Fact]
    public async Task 未認証のユーザーなら確認トークンを発行して確認メールをOutboxに積む()
    {
        var user = SetupUser();

        await this.service.ExecuteAsync(user.Id);

        var verification = Assert.Single(this.createdVerifications);
        Assert.Equal(user.Id, verification.UserId);
        Assert.Equal(Now.AddHours(24), verification.ExpiresAt);
        var message = Assert.Single(this.outboxMessages);
        Assert.Equal(OutboxMessage.EmailType, message.Type);
        Assert.Contains(verification.Token, message.Payload);
    }

    [Fact]
    public async Task 直近の送信から60秒未満なら再送しない()
    {
        var user = SetupUser();
        SetupLatestVerification(user, Now.AddSeconds(-59));

        await Assert.ThrowsAsync<TooManyRequestsException>(() => this.service.ExecuteAsync(user.Id));

        Assert.Empty(this.createdVerifications);
        Assert.Empty(this.outboxMessages);
    }

    [Fact]
    public async Task 直近の送信からちょうど60秒経過していれば再送する()
    {
        var user = SetupUser();
        SetupLatestVerification(user, Now.AddSeconds(-60));

        await this.service.ExecuteAsync(user.Id);

        Assert.Single(this.createdVerifications);
        Assert.Single(this.outboxMessages);
    }

    [Fact]
    public async Task 認証済みのユーザーには再送しない()
    {
        var user = SetupUser(true);

        await Assert.ThrowsAsync<ConflictException>(() => this.service.ExecuteAsync(user.Id));

        Assert.Empty(this.createdVerifications);
        Assert.Empty(this.outboxMessages);
    }
}
