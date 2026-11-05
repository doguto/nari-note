using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class ForgotPasswordServiceTest
{
    const string Email = "taro@example.com";

    readonly IOutboxMessageRepository outboxMessageRepository = Substitute.For<IOutboxMessageRepository>();
    readonly IPasswordResetTokenRepository passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
    readonly ForgotPasswordService service;
    readonly TestTimeProvider timeProvider = new();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public ForgotPasswordServiceTest()
    {
        this.service = new ForgotPasswordService(
            this.userRepository,
            this.passwordResetTokenRepository,
            this.outboxMessageRepository,
            new EmailMessageStore("https://nari-note.com"),
            this.timeProvider
        );
    }

    [Fact]
    public async Task 登録済みのメールアドレスなら1時間有効な再設定トークンとメールを作成する()
    {
        var user = new UserBuilder().WithEmail(Email).Build();
        this.userRepository.FindByEmailAsync(Email).Returns(user);
        PasswordResetToken? created = null;
        await this.passwordResetTokenRepository.CreateAsync(Arg.Do<PasswordResetToken>(t => created = t));

        await this.service.ExecuteAsync(new ForgotPasswordRequest { Email = Email });

        Assert.NotNull(created);
        Assert.Equal(user.Id, created.UserId);
        Assert.Equal(TestTimeProvider.DefaultUtcNow.AddHours(1), created.ExpiresAt);
        await this.outboxMessageRepository.Received(1).AddAsync(
            Arg.Is<OutboxMessage>(m => m.Type == OutboxMessage.EmailType && m.Payload.Contains(created.Token))
        );
    }

    [Fact]
    public async Task 直近の送信から60秒経過するまでは再設定メールを送らない()
    {
        var user = new UserBuilder().WithEmail(Email).Build();
        this.userRepository.FindByEmailAsync(Email).Returns(user);
        var latest = TestEntity.PasswordResetToken(user, "latest-token", TestTimeProvider.DefaultUtcNow.AddHours(1));
        latest.CreatedAt = TestTimeProvider.DefaultUtcNow.AddSeconds(-59);
        this.passwordResetTokenRepository.FindLatestByUserIdAsync(user.Id).Returns(latest);

        // 登録有無が漏れないよう、エラーにはせず送信だけを止める
        await this.service.ExecuteAsync(new ForgotPasswordRequest { Email = Email });
        await this.passwordResetTokenRepository.DidNotReceive().CreateAsync(Arg.Any<PasswordResetToken>());
        await this.outboxMessageRepository.DidNotReceive().AddAsync(Arg.Any<OutboxMessage>());

        this.timeProvider.Advance(TimeSpan.FromSeconds(1));
        await this.service.ExecuteAsync(new ForgotPasswordRequest { Email = Email });

        await this.passwordResetTokenRepository.Received(1).CreateAsync(Arg.Any<PasswordResetToken>());
        await this.outboxMessageRepository.Received(1).AddAsync(Arg.Any<OutboxMessage>());
    }

    [Fact]
    public async Task 未登録のメールアドレスなら何も作成しない()
    {
        await this.service.ExecuteAsync(new ForgotPasswordRequest { Email = Email });

        await this.passwordResetTokenRepository.DidNotReceive().CreateAsync(Arg.Any<PasswordResetToken>());
        await this.outboxMessageRepository.DidNotReceive().AddAsync(Arg.Any<OutboxMessage>());
    }
}
