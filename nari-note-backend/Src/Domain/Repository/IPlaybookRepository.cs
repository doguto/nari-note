using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

// 定跡は局面・手・形グループ・候補をまとめて扱うため、関連の追加と削除もこの Repository で行う
public interface IPlaybookRepository : IRepository<Playbook, PlaybookId>
{
    Task<List<PlaybookSummary>> FindSummariesByUserAsync(UserId userId);

    // 局面・手・形グループ・候補をすべて読み込んだ定跡。変更は SaveAsync で保存する
    Task<Playbook> FindForceGraphByIdAsync(PlaybookId id);

    void AddPosition(PlaybookPosition position);
    void AddMove(PlaybookMove move);
    void AddShapeGroup(ShapeGroup group);
    void AddShapeCandidate(ShapeCandidate candidate);
    void RemovePosition(PlaybookPosition position);
    void RemoveMove(PlaybookMove move);
    void RemoveShapeGroup(ShapeGroup group);
    void RemoveShapeCandidate(ShapeCandidate candidate);
    Task SaveAsync();

    Task<int> CountPendingJudgesAsync(PlaybookId id);
    Task<List<PlaybookPosition>> FindUnscannedPositionsAsync(int limit);
    Task<List<PlaybookPosition>> FindPositionsAsync(PlaybookId playbookId);
    Task<ShapeCandidate?> FindNextQueuedCandidateAsync();
    Task ResetRunningCandidatesAsync();
}
