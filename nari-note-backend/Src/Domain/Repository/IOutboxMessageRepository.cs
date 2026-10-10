using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface IOutboxMessageRepository
{
    /// <summary>
    /// 送信予定を保存する。呼び出し元のトランザクションに参加する。
    /// </summary>
    Task AddAsync(OutboxMessage message);

    /// <summary>
    /// 送信対象のメッセージを最大 batchSize 件取得する。
    /// 取得と同時に Attempts を加算し、NextAttemptAt を lease 分だけ未来へ進めるため、
    /// 複数インスタンスで同時に動作しても同じメッセージを二重に取得しない。
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(int batchSize, DateTime now, TimeSpan lease);

    /// <summary>
    /// 送信結果（成功・失敗）を保存する。
    /// </summary>
    Task SaveResultAsync(OutboxMessage message);
}
