using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class DiscordWebhookNotifier : IDiscordNotifier
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    readonly HttpClient httpClient;
    readonly string webhookUrl;

    public DiscordWebhookNotifier(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        webhookUrl = configuration["discord_general_webhook_url"]
            ?? throw new InvalidOperationException("discord_general_webhook_url is not configured");
    }

    public async Task NotifyAsync(string message)
    {
        var payload = JsonSerializer.Serialize(new { content = message }, JsonOptions);
        await PostAsync(payload);
    }

    public async Task NotifyWithEmbedAsync(DiscordEmbed embed)
    {
        var payload = JsonSerializer.Serialize(new { embeds = new[] { embed } }, JsonOptions);
        await PostAsync(payload);
    }

    async Task PostAsync(string payload)
    {
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await httpClient.PostAsync(webhookUrl, content);
    }
}
