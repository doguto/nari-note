using NariNoteBackend.Domain.Template;

namespace NariNoteBackend.Domain.Gateway;

public record EmailMessage(
    string From,
    IList<string> To,
    string Subject,
    string? HtmlBody = null,
    string? TextBody = null
);

public class EmailMessageStore
{
    const string From = "noreply@email.nari-note.com";

    // メール本文のリンク先となるフロントエンドの URL（環境ごとの appsettings で指定する）
    readonly string frontendBaseUrl;

    public EmailMessageStore(string frontendBaseUrl)
    {
        this.frontendBaseUrl = frontendBaseUrl.TrimEnd('/');
    }

    public EmailMessage SignupMessage(string to, Guid guid)
    {
        var url = $"{this.frontendBaseUrl}/validate?token={guid}";

        return new EmailMessage(
            From,
            [to],
            "【なりノート】メールアドレスの確認",
            EmailTemplate.SignupVerificationHtml(url),
            EmailTemplate.SignupVerificationText(url)
        );
    }

    public EmailMessage AlreadyRegisteredMessage(string to)
    {
        var loginUrl = $"{this.frontendBaseUrl}/login";
        var forgotPasswordUrl = $"{this.frontendBaseUrl}/forgot-password";

        return new EmailMessage(
            From,
            [to],
            "【なりノート】このメールアドレスは登録済みです",
            EmailTemplate.AlreadyRegisteredHtml(loginUrl, forgotPasswordUrl),
            EmailTemplate.AlreadyRegisteredText(loginUrl, forgotPasswordUrl)
        );
    }

    public EmailMessage ForgotPasswordMessage(string to, Guid guid)
    {
        var url = $"{this.frontendBaseUrl}/reset-password?token={guid}";

        return new EmailMessage(
            From,
            [to],
            "【なりノート】パスワードの再設定",
            EmailTemplate.PasswordResetHtml(url),
            EmailTemplate.PasswordResetText(url)
        );
    }
}
