using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Domain.Entity;

/// <summary>
/// 外部サービス（メール・Discord）への送信予定を保持する Outbox。
/// 業務データと同一トランザクションで保存し、BackgroundService が Commit 後に送信する。
/// </summary>
[Index(nameof(ProcessedAt), nameof(FailedAt), nameof(NextAttemptAt))]
public class OutboxMessage : EntityBase
{
    public const string EmailType = "Email";
    public const string DiscordEmbedType = "DiscordEmbed";

    // 最大試行回数に達したメッセージは FailedAt を記録して再試行を止める
    public const int MaxAttempts = 5;

    // ID は高頻度の同時 INSERT で衝突しないよう、採番を DB に依存しない Guid とする
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(32)]
    public required string Type { get; set; }

    // 送信内容を JSON 化したもの
    [Required]
    public required string Payload { get; set; }

    // 次に取得可能となる時刻。取得時に未来へ進めることで、他ワーカーとの重複取得を防ぐ
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    public int Attempts { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // 最大試行回数に達して断念した時刻
    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }

    public static OutboxMessage ForEmail(EmailMessage message)
    {
        return new OutboxMessage { Type = EmailType, Payload = JsonSerializer.Serialize(message) };
    }

    public static OutboxMessage ForDiscordEmbed(DiscordEmbed embed)
    {
        return new OutboxMessage { Type = DiscordEmbedType, Payload = JsonSerializer.Serialize(embed) };
    }

    public void MarkProcessed(DateTime now)
    {
        ProcessedAt = now;
        LastError = null;
        UpdatedAt = now;
    }

    public void MarkFailed(DateTime now, string error)
    {
        LastError = error.Length > 1000 ? error[..1000] : error;
        UpdatedAt = now;

        if (Attempts >= MaxAttempts)
        {
            FailedAt = now;
            return;
        }

        NextAttemptAt = now + OutboxRetryPolicy.CalculateDelay(Attempts);
    }
}
