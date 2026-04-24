using System.Text.Json.Serialization;

namespace NariNoteBackend.Domain.Gateway;

public record DiscordEmbed
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int? Color { get; init; }
    public string? Timestamp { get; init; }
    public DiscordEmbedFooter? Footer { get; init; }
    public IReadOnlyList<DiscordEmbedField>? Fields { get; init; }
}

public record DiscordEmbedFooter(string Text);

public record DiscordEmbedField(string Name, string Value, bool Inline = false);
