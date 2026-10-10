using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);

    /// <summary>
    /// 取得と同時に Attempts を加算し NextAttemptAt を lease 分進めるため、複数インスタンスでも二重取得しない
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(int batchSize, DateTime now, TimeSpan lease);

    Task SaveResultAsync(OutboxMessage message);
}
