using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.BackgroundJob;

/// <summary>
/// サインアップジョブの実処理
/// メールアドレスの登録有無による分岐はレスポンス返却後にここで行う（ユーザー列挙攻撃防止）
/// </summary>
public class SignUpJobHandler
{
    readonly IEmailHelper emailHelper;
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IUserRepository userRepository;
    readonly IDiscordNotifier discordNotifier;

    public SignUpJobHandler(
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

    public async Task ExecuteAsync(SignUpJob job)
    {
        var existingUser = await userRepository.FindByEmailAsync(job.Email);
        if (existingUser == null)
        {
            await CreateUserAsync(job);
            return;
        }

        if (existingUser.IsEmailVerified)
        {
            // 登録済みアドレスには、その旨とパスワード再設定の案内を通知する
            await emailHelper.SendAsync(EmailMessageStore.AlreadyRegisteredMessage(existingUser.Email));
            return;
        }

        // 未認証のアカウントは確認メールを再送する（既存の名前・パスワードは変更しない）
        await SendVerificationEmailAsync(existingUser);
    }

    async Task CreateUserAsync(SignUpJob job)
    {
        var user = new User
        {
            Name = job.Name,
            Email = job.Email,
            PasswordHash = job.PasswordHash
        };

        var createdUser = await userRepository.CreateAsync(user);

        await SendVerificationEmailAsync(createdUser);

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
    }

    async Task SendVerificationEmailAsync(User user)
    {
        var guid = Guid.NewGuid();
        var emailVerification = new EmailVerification
        {
            UserId = user.Id,
            Token = guid.ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        await emailVerificationRepository.CreateAsync(emailVerification);

        await emailHelper.SendAsync(EmailMessageStore.SignupMessage(user.Email, guid));
    }
}
