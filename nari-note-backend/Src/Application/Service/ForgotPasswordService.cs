using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class ForgotPasswordService
{
    readonly IEmailHelper emailHelper;
    readonly IPasswordResetTokenRepository passwordResetTokenRepository;
    readonly IUserRepository userRepository;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailHelper emailHelper
    )
    {
        this.userRepository = userRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
        this.emailHelper = emailHelper;
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
            Token = tokenGuid.ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        await passwordResetTokenRepository.CreateAsync(passwordResetToken);

        var message = EmailMessageStore.ForgotPasswordMessage(user.Email, tokenGuid);
        await emailHelper.SendAsync(message);

        return new ForgotPasswordResponse();
    }
}
