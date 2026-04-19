using NariNoteBackend.Domain.Template;

namespace NariNoteBackend.Domain.Gateway;

public record EmailMessage(
    string From,
    IList<string> To,
    string Subject,
    string? HtmlBody = null,
    string? TextBody = null
);

public static class EmailMessageStore
{
    const string From = "noreply@email.nari-note.com";

    public static EmailMessage SignupMessage(string to, Guid guid)
    {
        var url = $"https://nari-note.com/validate?token={guid}";

        return new EmailMessage(
            From,
            [to],
            "【なりノート】メールアドレスの確認",
            EmailTemplate.SignupVerificationHtml(url),
            EmailTemplate.SignupVerificationText(url)
        );
    }

    public static EmailMessage ForgotPasswordMessage(string to, Guid guid)
    {
        var url = $"https://nari-note.com/reset-password?token={guid}";

        return new EmailMessage(
            From,
            [to],
            "【なりノート】パスワードの再設定",
            EmailTemplate.PasswordResetHtml(url),
            EmailTemplate.PasswordResetText(url)
        );
    }
}
