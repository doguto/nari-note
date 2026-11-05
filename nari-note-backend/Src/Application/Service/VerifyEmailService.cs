using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class VerifyEmailService
{
    readonly ICookieOptionsHelper cookieOptionsHelper;
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IJwtHelper jwtHelper;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public VerifyEmailService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IJwtHelper jwtHelper,
        ICookieOptionsHelper cookieOptionsHelper,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.jwtHelper = jwtHelper;
        this.cookieOptionsHelper = cookieOptionsHelper;
        this.timeProvider = timeProvider;
    }

    public async Task<AuthResponse> ExecuteAsync(VerifyEmailRequest request, HttpResponse response)
    {
        var emailVerification = await emailVerificationRepository.FindByTokenAsync(request.Token);
        if (emailVerification == null) throw new ArgumentException("無効なトークンです");
        if (emailVerification.IsUsed) throw new ArgumentException("このトークンは既に使用されています");
        if (emailVerification.ExpiresAt < timeProvider.UtcNow()) throw new ArgumentException("トークンの有効期限が切れています");

        emailVerification.IsUsed = true;
        await emailVerificationRepository.UpdateAsync(emailVerification);

        var user = emailVerification.User;
        var isPasswordChanged = await ApplySignUpInputAsync(user, emailVerification);
        user.IsEmailVerified = true;
        // パスワードが置き換わる場合は、以前のパスワードで発行された認証トークンを無効にする
        // （メールアドレスの持ち主ではない人が先に登録・サインインしている可能性があるため）
        if (isPasswordChanged) user.RotateSecurityStamp();
        await userRepository.UpdateAsync(user);

        var token = jwtHelper.GenerateToken(user);

        var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(
            TimeSpan.FromHours(jwtHelper.GetExpiration())
        );
        response.Cookies.Append("authToken", token, cookieOptions);

        return new AuthResponse { UserId = user.Id, IsEmailVerified = user.IsEmailVerified };
    }

    // サインアップ時に入力されたユーザー名・パスワードを反映する。パスワードが変わった場合は true を返す
    async Task<bool> ApplySignUpInputAsync(User user, EmailVerification emailVerification)
    {
        var isPasswordChanged = emailVerification.PasswordHash != null && emailVerification.PasswordHash != user.PasswordHash;
        if (isPasswordChanged) user.PasswordHash = emailVerification.PasswordHash!;

        // サインアップ後に他のユーザーが同じ名前を使い始めた場合は、現在の名前のままにする
        var name = emailVerification.Name;
        if (name != null && name != user.Name && !await userRepository.ExistsByNameAsync(name, user.Id))
        {
            user.Name = name;
        }

        return isPasswordChanged;
    }
}
