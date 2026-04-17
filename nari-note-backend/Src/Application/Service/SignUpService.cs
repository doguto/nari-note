using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IEmailHelper emailHelper;
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IUserRepository userRepository;

    public SignUpService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailHelper emailHelper
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.emailHelper = emailHelper;
    }

    public async Task<AuthResponse> ExecuteAsync(SignUpRequest request)
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
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        await emailVerificationRepository.CreateAsync(emailVerification);

        await emailHelper.SendAsync(EmailMessageStore.SignupMessage(request.Email, guid));

        return new AuthResponse
        {
            UserId = createdUser.Id
        };
    }
}
