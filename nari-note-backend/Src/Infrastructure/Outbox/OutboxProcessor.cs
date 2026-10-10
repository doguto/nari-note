using System.Text.Json;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Infrastructure.Outbox;

/// <summary>
/// Outbox の未送信メッセージを 1 バッチ分処理する。
/// 取得・送信・結果保存をそれぞれ独立した DB 操作とし、外部 I/O の間は DB 接続・行ロックを保持しない。
/// </summary>
public class OutboxProcessor
{
    public const int BatchSize = 20;

    // 取得したメッセージの処理中に、他のインスタンスから再取得されないための猶予時間
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

    /// <returns>処理を試みたメッセージ数</returns>
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

        // 結果の保存に失敗した場合は Lease 経過後に再取得される（少なくとも 1 回の配信）
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
