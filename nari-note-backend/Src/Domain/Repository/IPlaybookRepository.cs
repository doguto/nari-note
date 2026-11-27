using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

// 定跡はノード・局面・遷移をまとめて扱うため、関連の追加と削除もこの Repository で行う
public interface IPlaybookRepository : IRepository<Playbook, PlaybookId>
{
    Task<List<PlaybookSummary>> FindSummariesByUserAsync(UserId userId);

    // ノード・局面・遷移をすべて読み込んだ定跡。変更は SaveAsync で保存する
    Task<Playbook> FindForceGraphByIdAsync(PlaybookId id);

    void AddNode(PlaybookNode node);
    void AddBoard(PlaybookBoard board);
    void AddTransition(PlaybookBoardTransition transition);
    void RemoveNode(PlaybookNode node);
    void RemoveBoard(PlaybookBoard board);
    void RemoveTransition(PlaybookBoardTransition transition);
    Task SaveAsync();
}
