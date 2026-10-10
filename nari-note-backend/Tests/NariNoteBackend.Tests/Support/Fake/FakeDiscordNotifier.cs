using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeDiscordNotifier : IDiscordNotifier
{
    public List<string> Messages { get; } = new();
    public List<DiscordEmbed> Embeds { get; } = new();

    public Task NotifyAsync(string message)
    {
        this.Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task NotifyWithEmbedAsync(DiscordEmbed embed)
    {
        this.Embeds.Add(embed);
        return Task.CompletedTask;
    }
}
