using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;

namespace NariNoteBackend.Application.Service;

public class VerifyEmailService
{
    readonly ICookieOptionsHelper cookieOptionsHelper;
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IJwtHelper jwtHelper;
    readonly IUserRepository userRepository;

    public VerifyEmailService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IJwtHelper jwtHelper,
        ICookieOptionsHelper cookieOptionsHelper
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.jwtHelper = jwtHelper;
        this.cookieOptionsHelper = cookieOptionsHelper;
    }

    public async Task<AuthResponse> ExecuteAsync(VerifyEmailRequest request, HttpResponse response)
    {
        var emailVerification = await emailVerificationRepository.FindByTokenAsync(request.Token);
        if (emailVerification == null) throw new ArgumentException("無効なトークンです");
        if (emailVerification.IsUsed) throw new ArgumentException("このトークンは既に使用されています");
        if (emailVerification.ExpiresAt < DateTime.Now) throw new ArgumentException("トークンの有効期限が切れています");

        emailVerification.IsUsed = true;
        await emailVerificationRepository.UpdateAsync(emailVerification);

        var user = emailVerification.User;
        user.IsEmailVerified = true;
        var userUpdateTask = userRepository.UpdateAsync(user);

        var token = jwtHelper.GenerateToken(user.Id, user.Name);

        await userUpdateTask;

        var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(
            TimeSpan.FromHours(jwtHelper.GetExpirationInHours())
        );
        response.Cookies.Append("authToken", token, cookieOptions);

        return new AuthResponse { UserId = user.Id };
    }
}
