using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class ForgotPasswordService
{
    readonly EmailMessageStore emailMessageStore;
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly IPasswordResetTokenRepository passwordResetTokenRepository;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IOutboxMessageRepository outboxMessageRepository,
        EmailMessageStore emailMessageStore,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
        this.outboxMessageRepository = outboxMessageRepository;
        this.emailMessageStore = emailMessageStore;
        this.timeProvider = timeProvider;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request)
    {
        var user = await userRepository.FindByEmailAsync(request.Email);

        // メールが存在しない場合でも同じレスポンスを返す（ユーザー列挙攻撃防止）
        if (user == null) return new ForgotPasswordResponse();

        // 同じアドレスへの連続送信を防ぐ。登録有無が漏れないよう、エラーにはせず送信だけを止める
        var now = timeProvider.UtcNow();
        var latest = await passwordResetTokenRepository.FindLatestByUserIdAsync(user.Id);
        if (AuthEmailCooldown.IsActive(latest?.CreatedAt, now)) return new ForgotPasswordResponse();

        var tokenGuid = Guid.NewGuid();
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = tokenGuid.ToString(),
            ExpiresAt = now.AddHours(1),
        };

        await passwordResetTokenRepository.CreateAsync(passwordResetToken);

        var message = this.emailMessageStore.ForgotPasswordMessage(user.Email, tokenGuid);
        await outboxMessageRepository.AddAsync(OutboxMessage.ForEmail(message, now));

        return new ForgotPasswordResponse();
    }
}
