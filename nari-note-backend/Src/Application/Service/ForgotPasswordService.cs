using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class ForgotPasswordService
{
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly IPasswordResetTokenRepository passwordResetTokenRepository;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IOutboxMessageRepository outboxMessageRepository,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
        this.outboxMessageRepository = outboxMessageRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request)
    {
        var user = await userRepository.FindByEmailAsync(request.Email);

        // メールが存在しない場合でも同じレスポンスを返す（ユーザー列挙攻撃防止）
        if (user == null) return new ForgotPasswordResponse();

        var now = timeProvider.UtcNow();
        var tokenGuid = Guid.NewGuid();
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = tokenGuid.ToString(),
            ExpiresAt = now.AddHours(1),
        };

        await passwordResetTokenRepository.CreateAsync(passwordResetToken);

        var message = EmailMessageStore.ForgotPasswordMessage(user.Email, tokenGuid);
        await outboxMessageRepository.AddAsync(OutboxMessage.ForEmail(message, now));

        return new ForgotPasswordResponse();
    }
}
