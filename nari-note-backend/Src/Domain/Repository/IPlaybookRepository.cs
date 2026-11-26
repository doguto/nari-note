using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

// 定跡はノード・盤面・手順をまとめて扱うため、関連の追加と削除もこの Repository で行う
public interface IPlaybookRepository : IRepository<Playbook, PlaybookId>
{
    Task<List<PlaybookSummary>> FindSummariesByUserAsync(UserId userId);

    // ノード・盤面・手順をすべて読み込んだ定跡。変更は SaveAsync で保存する
    Task<Playbook> FindForceGraphByIdAsync(PlaybookId id);

    void AddNode(PlaybookNode node);
    void AddBoard(PlaybookNodeBoard board);
    void AddStep(PlaybookNodeStep step);
    void RemoveNode(PlaybookNode node);
    void RemoveBoard(PlaybookNodeBoard board);
    void RemoveStep(PlaybookNodeStep step);
    Task SaveAsync();
}
