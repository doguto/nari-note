using System.Text.Json;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Infrastructure.Outbox;

public class OutboxProcessor
{
    public const int BatchSize = 20;

    public static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);

    readonly IDiscordNotifier discordNotifier;
    readonly IEmailHelper emailHelper;
    readonly ILogger<OutboxProcessor> logger;
    readonly IOutboxMessageRepository outboxMessageRepository;

    public OutboxProcessor(
        IOutboxMessageRepository outboxMessageRepository,
        IEmailHelper emailHelper,
        IDiscordNotifier discordNotifier,
        ILogger<OutboxProcessor> logger
    )
    {
        this.outboxMessageRepository = outboxMessageRepository;
        this.emailHelper = emailHelper;
        this.discordNotifier = discordNotifier;
        this.logger = logger;
    }

    public async Task<int> ProcessBatchAsync()
    {
        var messages = await outboxMessageRepository.ClaimPendingAsync(BatchSize, DateTime.UtcNow, Lease);

        foreach (var message in messages)
        {
            await ProcessAsync(message);
        }

        return messages.Count;
    }

    async Task ProcessAsync(OutboxMessage message)
    {
        try
        {
            await DispatchAsync(message);
            message.MarkProcessed(DateTime.UtcNow);
        }
        catch (System.Exception ex)
        {
            message.MarkFailed(DateTime.UtcNow, ex.Message);
            logger.LogWarning(
                ex,
                "Outbox message failed. Id={Id} Type={Type} Attempts={Attempts} Abandoned={Abandoned}",
                message.Id,
                message.Type,
                message.Attempts,
                message.FailedAt != null
            );
        }

        await outboxMessageRepository.SaveResultAsync(message);
    }

    async Task DispatchAsync(OutboxMessage message)
    {
        switch (message.Type)
        {
            case OutboxMessage.EmailType:
                var email = JsonSerializer.Deserialize<EmailMessage>(message.Payload)
                            ?? throw new InvalidOperationException("Outbox email payload is empty");
                await emailHelper.SendAsync(email);
                break;
            case OutboxMessage.DiscordEmbedType:
                var embed = JsonSerializer.Deserialize<DiscordEmbed>(message.Payload)
                            ?? throw new InvalidOperationException("Outbox discord payload is empty");
                await discordNotifier.NotifyWithEmbedAsync(embed);
                break;
            default:
                throw new InvalidOperationException($"Unknown outbox message type: {message.Type}");
        }
    }
}
