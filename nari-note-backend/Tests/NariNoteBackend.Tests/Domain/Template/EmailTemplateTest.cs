using NariNoteBackend.Domain.Template;

namespace NariNoteBackend.Tests.Domain.Template;

public class EmailTemplateTest
{
    const string Url = "https://nari-note.com/example?token=abc";
    const string OtherUrl = "https://nari-note.com/other";

    public static TheoryData<string, string[]> Templates => new()
    {
        { EmailTemplate.SignupVerificationHtml(Url), [Url] },
        { EmailTemplate.SignupVerificationText(Url), [Url] },
        { EmailTemplate.AlreadyRegisteredHtml(Url, OtherUrl), [Url, OtherUrl] },
        { EmailTemplate.AlreadyRegisteredText(Url, OtherUrl), [Url, OtherUrl] },
        { EmailTemplate.PasswordResetHtml(Url), [Url] },
        { EmailTemplate.PasswordResetText(Url), [Url] }
    };

    [Theory]
    [MemberData(nameof(Templates))]
    public void プレースホルダーがすべて置換される(string body, string[] expectedUrls)
    {
        Assert.DoesNotContain("{{", body);
        Assert.DoesNotContain("}}", body);
        foreach (var url in expectedUrls) Assert.Contains(url, body);
        Assert.Contains("© 2026 なりノート", body);
    }

    [Fact]
    public void HTMLでは値がエスケープされる()
    {
        var body = EmailTemplate.PasswordResetHtml("https://nari-note.com/?a=1&b=<script>");

        Assert.Contains("https://nari-note.com/?a=1&amp;b=&lt;script&gt;", body);
        Assert.DoesNotContain("<script>", body);
    }
}
