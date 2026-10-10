using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class ForgotPasswordService
{
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly IPasswordResetTokenRepository passwordResetTokenRepository;
    readonly IUserRepository userRepository;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IOutboxMessageRepository outboxMessageRepository
    )
    {
        this.userRepository = userRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
        this.outboxMessageRepository = outboxMessageRepository;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request)
    {
        var user = await userRepository.FindByEmailAsync(request.Email);

        // メールが存在しない場合でも同じレスポンスを返す（ユーザー列挙攻撃防止）
        if (user == null) return new ForgotPasswordResponse();

        var tokenGuid = Guid.NewGuid();
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = tokenGuid.ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        await passwordResetTokenRepository.CreateAsync(passwordResetToken);

        // メール送信は Outbox に保存し、Commit 後にワーカーが送信する
        var message = EmailMessageStore.ForgotPasswordMessage(user.Email, tokenGuid);
        await outboxMessageRepository.AddAsync(OutboxMessage.ForEmail(message));

        return new ForgotPasswordResponse();
    }
}
