using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly ICookieOptionsHelper cookieOptionsHelper;
    readonly IEmailHelper emailHelper;
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IJwtHelper jwtHelper;

    readonly IUserRepository userRepository;

    public SignUpService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IJwtHelper jwtHelper,
        ICookieOptionsHelper cookieOptionsHelper,
        IEmailHelper emailHelper
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.jwtHelper = jwtHelper;
        this.cookieOptionsHelper = cookieOptionsHelper;
        this.emailHelper = emailHelper;
    }

    public async Task<AuthResponse> ExecuteAsync(SignUpRequest request, HttpResponse response)
    {
        var existingUser = await userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null) throw new ArgumentException("このメールアドレスは既に使用されています");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        var createdUser = await userRepository.CreateAsync(user);

        var guid = Guid.NewGuid();
        var emailVerification = new EmailVerification
        {
            UserId = createdUser.Id,
            Token = guid.ToString(),
            ExpiresAt = DateTime.Now.AddHours(24)
        };
        await emailVerificationRepository.CreateAsync(emailVerification);

        // var token = jwtHelper.GenerateToken(createdUser.Id, user.Name);

        // HttpOnly Cookieにトークンを設定
        // var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(TimeSpan.FromHours(jwtHelper.GetExpirationInHours()));
        // response.Cookies.Append("authToken", token, cookieOptions);

        await emailHelper.SendAsync(EmailMessageStore.SignupMessage(request.Email, guid));

        return new AuthResponse
        {
            UserId = createdUser.Id
        };
    }
}
