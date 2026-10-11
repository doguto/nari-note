using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class ResendVerificationEmailService
{
    // 連続送信を防ぐため、直近の送信からこの時間が経過するまで再送を拒否する
    static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public ResendVerificationEmailService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.outboxMessageRepository = outboxMessageRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<ResendVerificationEmailResponse> ExecuteAsync(UserId userId)
    {
        var user = await userRepository.FindForceByIdAsync(userId);
        if (user.IsEmailVerified) throw new ConflictException("メールアドレスは既に認証済みです");

        var now = timeProvider.UtcNow();
        var latest = await emailVerificationRepository.FindLatestByUserIdAsync(userId);
        if (latest != null && now - latest.CreatedAt < ResendCooldown)
        {
            throw new TooManyRequestsException("確認メールは送信済みです。しばらくしてから再度お試しください。");
        }

        // 発行済みのトークンは失効させない（有効期限内であれば、どの確認メールのリンクからでも認証できる）
        var guid = Guid.NewGuid();
        var emailVerification = new EmailVerification
        {
            UserId = user.Id,
            Token = guid.ToString(),
            ExpiresAt = now.AddHours(24)
        };
        await emailVerificationRepository.CreateAsync(emailVerification);

        await outboxMessageRepository.AddAsync(
            OutboxMessage.ForEmail(EmailMessageStore.SignupMessage(user.Email, guid), now)
        );

        return new ResendVerificationEmailResponse();
    }
}
