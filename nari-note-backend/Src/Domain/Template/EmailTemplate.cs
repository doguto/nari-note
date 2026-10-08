using System.Collections.Concurrent;
using System.Net;
using System.Reflection;

namespace NariNoteBackend.Domain.Template;

/// <summary>
/// メール本文のテンプレート
/// 本文は Email/ 配下の .html / .txt ファイル（埋め込みリソース）で管理し、
/// {{key}} 形式のプレースホルダーを置換して生成する
/// </summary>
public static class EmailTemplate
{
    const string ResourcePrefix = "NariNoteBackend.Domain.Template.Email.";

    static readonly ConcurrentDictionary<string, string> Cache = new();

    public static string SignupVerificationHtml(string verificationUrl)
    {
        return RenderHtml("SignupVerification.html", ("verificationUrl", verificationUrl));
    }

    public static string SignupVerificationText(string verificationUrl)
    {
        return RenderText("SignupVerification.txt", ("verificationUrl", verificationUrl));
    }

    public static string AlreadyRegisteredHtml(string loginUrl, string forgotPasswordUrl)
    {
        return RenderHtml(
            "AlreadyRegistered.html",
            ("loginUrl", loginUrl),
            ("forgotPasswordUrl", forgotPasswordUrl)
        );
    }

    public static string AlreadyRegisteredText(string loginUrl, string forgotPasswordUrl)
    {
        return RenderText(
            "AlreadyRegistered.txt",
            ("loginUrl", loginUrl),
            ("forgotPasswordUrl", forgotPasswordUrl)
        );
    }

    public static string PasswordResetHtml(string resetUrl)
    {
        return RenderHtml("PasswordReset.html", ("resetUrl", resetUrl));
    }

    public static string PasswordResetText(string resetUrl)
    {
        return RenderText("PasswordReset.txt", ("resetUrl", resetUrl));
    }

    static string RenderHtml(string fileName, params (string Key, string Value)[] values)
    {
        return Render(fileName, values.Select(v => (v.Key, WebUtility.HtmlEncode(v.Value))).ToArray());
    }

    static string RenderText(string fileName, params (string Key, string Value)[] values)
    {
        return Render(fileName, values);
    }

    static string Render(string fileName, (string Key, string Value)[] values)
    {
        var template = Cache.GetOrAdd(fileName, Load);
        foreach (var (key, value) in values)
        {
            template = template.Replace($"{{{{{key}}}}}", value);
        }

        return template;
    }

    static string Load(string fileName)
    {
        var resourceName = ResourcePrefix + fileName;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"メールテンプレートが見つかりません: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
