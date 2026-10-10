using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeEmailHelper : IEmailHelper
{
    public List<EmailMessage> SentMessages { get; } = new();

    public Task SendAsync(EmailMessage message)
    {
        this.SentMessages.Add(message);
        return Task.CompletedTask;
    }
}
