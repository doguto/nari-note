using System.Text.Json;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;
using Serilog.Context;

namespace NariNoteBackend.Infrastructure.Outbox;

public class OutboxProcessor
{
    public const int BatchSize = 20;

    // ポーリング中のログに付与するプロパティ名。Program.cs の Serilog フィルタで出力対象から除外する
    public const string PollingLogProperty = "OutboxPolling";

    public static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);

    readonly IDiscordNotifier discordNotifier;
    readonly IEmailHelper emailHelper;
    readonly ILogger<OutboxProcessor> logger;
    readonly IOutboxMessageRepository outboxMessageRepository;
    readonly TimeProvider timeProvider;

    public OutboxProcessor(
        IOutboxMessageRepository outboxMessageRepository,
        IEmailHelper emailHelper,
        IDiscordNotifier discordNotifier,
        ILogger<OutboxProcessor> logger,
        TimeProvider timeProvider
    )
    {
        this.outboxMessageRepository = outboxMessageRepository;
        this.emailHelper = emailHelper;
        this.discordNotifier = discordNotifier;
        this.logger = logger;
        this.timeProvider = timeProvider;
    }

    public async Task<int> ProcessBatchAsync()
    {
        var messages = await ClaimPendingAsync();
        if (messages.Count == 0) return 0;

        foreach (var message in messages)
        {
            await ProcessAsync(message);
        }

        logger.LogInformation(
            "Outbox batch processed. Count={Count} Succeeded={Succeeded}",
            messages.Count,
            messages.Count(message => message.ProcessedAt != null)
        );

        return messages.Count;
    }

    async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync()
    {
        using (LogContext.PushProperty(PollingLogProperty, true))
        {
            return await outboxMessageRepository.ClaimPendingAsync(BatchSize, timeProvider.UtcNow(), Lease);
        }
    }

    async Task ProcessAsync(OutboxMessage message)
    {
        try
        {
            await DispatchAsync(message);
            message.MarkProcessed(timeProvider.UtcNow());
        }
        catch (System.Exception ex)
        {
            message.MarkFailed(timeProvider.UtcNow(), ex.Message);
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
