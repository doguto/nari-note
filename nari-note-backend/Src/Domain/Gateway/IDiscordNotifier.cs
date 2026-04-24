namespace NariNoteBackend.Domain.Gateway;

public interface IDiscordNotifier
{
    Task NotifyAsync(string message);
    Task NotifyWithEmbedAsync(DiscordEmbed embed);
}
