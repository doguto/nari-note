using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IEmailVerificationRepository emailVerificationRepository;
    readonly EmailMessageStore emailMessageStore;
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public SignUpService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        EmailMessageStore emailMessageStore,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.emailVerificationRepository = emailVerificationRepository;
        this.outboxMessageRepository = outboxMessageRepository;
        this.emailMessageStore = emailMessageStore;
        this.timeProvider = timeProvider;
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
                OutboxMessage.ForEmail(
                    this.emailMessageStore.AlreadyRegisteredMessage(existingUser.Email),
                    timeProvider.UtcNow()
                )
            );
        }
        else
        {
            // 未認証のアカウントは、今回の入力内容を載せた確認メールを送る
            // 名前・パスワードは確認メールのリンクで認証されたときに反映する（メールアドレスの持ち主が入力した値を使うため）
            if (await userRepository.ExistsByNameAsync(request.Name, existingUser.Id))
            {
                throw new ConflictException("このユーザー名は既に使用されています");
            }

            await SendVerificationEmailAsync(existingUser, request.Name, HashPassword(request.Password));
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

        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = User.NormalizeEmail(request.Email),
            PasswordHash = passwordHash
        };

        var createdUser = await userRepository.CreateAsync(user);

        await SendVerificationEmailAsync(createdUser, createdUser.Name, createdUser.PasswordHash);

        var now = timeProvider.UtcNow();
        await outboxMessageRepository.AddAsync(OutboxMessage.ForDiscordEmbed(
            new DiscordEmbed
            {
                Title = "新規ユーザー登録",
                Description = "新しいユーザーが nari-note に登録しました！",
                Color = 0x57F287,
                Timestamp = now.ToString("o"),
                Fields =
                [
                    new DiscordEmbedField("名前", createdUser.Name, Inline: true)
                ],
                Footer = new DiscordEmbedFooter("nari-note")
            },
            now
        ));
    }

    static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    async Task SendVerificationEmailAsync(User user, string name, string passwordHash)
    {
        var now = timeProvider.UtcNow();
        var guid = Guid.NewGuid();
        var emailVerification = new EmailVerification
        {
            UserId = user.Id,
            Token = guid.ToString(),
            ExpiresAt = now.AddHours(24),
            Name = name,
            PasswordHash = passwordHash
        };
        await emailVerificationRepository.CreateAsync(emailVerification);

        await outboxMessageRepository.AddAsync(
            OutboxMessage.ForEmail(this.emailMessageStore.SignupMessage(user.Email, guid), now)
        );
    }
}
