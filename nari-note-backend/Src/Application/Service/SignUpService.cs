using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly IUserRepository userRepository;

    public SignUpService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IOutboxMessageRepository outboxMessageRepository
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.outboxMessageRepository = outboxMessageRepository;
    }

    public async Task<SignUpResponse> ExecuteAsync(SignUpRequest request)
    {
        // メールアドレスの登録有無によらず同じレスポンスを返す（ユーザー列挙攻撃防止）
        var existingUser = await userRepository.FindByEmailAsync(request.Email);
        if (existingUser == null)
        {
            await CreateUserAsync(request);
        }
        else if (existingUser.IsEmailVerified)
        {
            // 登録済みアドレスには、その旨とパスワード再設定の案内を通知する
            await outboxMessageRepository.AddAsync(
                OutboxMessage.ForEmail(EmailMessageStore.AlreadyRegisteredMessage(existingUser.Email))
            );
        }
        else
        {
            // 未認証のアカウントは確認メールを再送する（既存の名前・パスワードは変更しない）
            await SendVerificationEmailAsync(existingUser);
        }

        return new SignUpResponse();
    }

    async Task CreateUserAsync(SignUpRequest request)
    {
        // 名前の重複確認は新規作成時のみ行う（既存メールの分岐に置くとメールの登録有無が漏れるため）
        if (await userRepository.ExistsByNameAsync(request.Name))
        {
            throw new ConflictException("このユーザー名は既に使用されています");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = User.NormalizeEmail(request.Email),
            PasswordHash = passwordHash
        };

        var createdUser = await userRepository.CreateAsync(user);

        await SendVerificationEmailAsync(createdUser);

        await outboxMessageRepository.AddAsync(OutboxMessage.ForDiscordEmbed(new DiscordEmbed
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
        }));
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

        await outboxMessageRepository.AddAsync(
            OutboxMessage.ForEmail(EmailMessageStore.SignupMessage(user.Email, guid))
        );
    }
}
