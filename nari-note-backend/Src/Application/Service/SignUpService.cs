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
    readonly IDiscordNotifier discordNotifier;

    public SignUpService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailHelper emailHelper,
        IDiscordNotifier discordNotifier
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.emailHelper = emailHelper;
        this.discordNotifier = discordNotifier;
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

        await discordNotifier.NotifyWithEmbedAsync(new DiscordEmbed
        {
            Title = "新規ユーザー登録",
            Description = "新しいユーザーが nari-note に登録しました！",
            Color = 0x57F287,
            Timestamp = DateTime.UtcNow.ToString("o"),
            Fields =
            [
                new DiscordEmbedField("名前", createdUser.Name, Inline: true)
            ],
            Footer = new DiscordEmbedFooter("nari-note")
        });

        return new AuthResponse
        {
            UserId = createdUser.Id
        };
    }
}
