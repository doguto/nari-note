using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

// 定跡は局面・手・形グループ・候補をまとめて扱うため、関連の追加と削除もこの Repository で行う
public interface IBookRepository : IRepository<Book, BookId>
{
    Task<List<BookSummary>> FindSummariesByUserAsync(UserId userId);

    // 局面・手・形グループ・候補をすべて読み込んだ定跡。変更は SaveAsync で保存する
    Task<Book> FindForceGraphByIdAsync(BookId id);

    void AddPosition(BookPosition position);
    void AddMove(BookMove move);
    void AddShapeGroup(ShapeGroup group);
    void AddShapeCandidate(ShapeCandidate candidate);
    void RemovePosition(BookPosition position);
    void RemoveMove(BookMove move);
    void RemoveShapeGroup(ShapeGroup group);
    void RemoveShapeCandidate(ShapeCandidate candidate);
    Task SaveAsync();

    Task<int> CountPendingJudgesAsync(BookId id);
    Task<List<BookPosition>> FindUnscannedPositionsAsync(int limit);
    Task<List<BookPosition>> FindPositionsAsync(BookId bookId);
    Task<ShapeCandidate?> FindNextQueuedCandidateAsync();
    Task ResetRunningCandidatesAsync();
}
