using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    readonly NariNoteDbContext context;

    public OutboxMessageRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(OutboxMessage message)
    {
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(int batchSize, DateTime now, TimeSpan lease)
    {
        var leaseUntil = now + lease;

        return await context.OutboxMessages
            .FromSql($"""
                UPDATE "OutboxMessages"
                SET "Attempts" = "Attempts" + 1,
                    "NextAttemptAt" = {leaseUntil}
                WHERE "Id" IN (
                    SELECT "Id"
                    FROM "OutboxMessages"
                    WHERE "ProcessedAt" IS NULL
                      AND "FailedAt" IS NULL
                      AND "NextAttemptAt" <= {now}
                    ORDER BY "CreatedAt"
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *
                """)
            .ToListAsync();
    }

    public async Task SaveResultAsync(OutboxMessage message)
    {
        context.OutboxMessages.Update(message);
        await context.SaveChangesAsync();
    }
}
