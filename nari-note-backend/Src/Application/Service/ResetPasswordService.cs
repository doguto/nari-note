using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class ResetPasswordService
{
    readonly IPasswordResetTokenRepository passwordResetTokenRepository;
    readonly IUserRepository userRepository;

    public ResetPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository
    )
    {
        this.userRepository = userRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
    }

    public async Task<ResetPasswordResponse> ExecuteAsync(ResetPasswordRequest request)
    {
        var passwordResetToken = await passwordResetTokenRepository.FindByTokenAsync(request.Token);
        if (passwordResetToken == null) throw new ArgumentException("無効なトークンです");
        if (passwordResetToken.IsUsed) throw new ArgumentException("このトークンは既に使用されています");
        if (passwordResetToken.ExpiresAt < DateTime.UtcNow) throw new ArgumentException("トークンの有効期限が切れています");

        passwordResetToken.IsUsed = true;
        await passwordResetTokenRepository.UpdateAsync(passwordResetToken);

        var user = passwordResetToken.User;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        return new ResetPasswordResponse();
    }
}
