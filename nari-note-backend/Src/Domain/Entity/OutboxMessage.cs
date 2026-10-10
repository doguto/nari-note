using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(ProcessedAt), nameof(FailedAt), nameof(NextAttemptAt))]
public class OutboxMessage : EntityBase
{
    public const string EmailType = "Email";
    public const string DiscordEmbedType = "DiscordEmbed";
    public const int MaxAttempts = 5;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(32)]
    public required string Type { get; set; }

    [Required]
    public required string Payload { get; set; }

    public required DateTime NextAttemptAt { get; set; }

    public int Attempts { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }

    public static OutboxMessage ForEmail(EmailMessage message, DateTime now)
    {
        return new OutboxMessage
        {
            Type = EmailType,
            Payload = JsonSerializer.Serialize(message),
            NextAttemptAt = now
        };
    }

    public static OutboxMessage ForDiscordEmbed(DiscordEmbed embed, DateTime now)
    {
        return new OutboxMessage
        {
            Type = DiscordEmbedType,
            Payload = JsonSerializer.Serialize(embed),
            NextAttemptAt = now
        };
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
