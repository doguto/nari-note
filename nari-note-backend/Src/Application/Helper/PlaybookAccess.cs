using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Helper;

public static class PlaybookAccess
{
    public static void EnsureOwner(Playbook playbook, UserId userId)
    {
        if (!playbook.IsOwnedBy(userId)) throw new ForbiddenException("この定跡を操作する権限がありません");
    }

    public static PlaybookBoard FindBoard(Playbook playbook, PlaybookBoardId id)
    {
        return playbook.Boards.FirstOrDefault(b => b.Id == id)
            ?? throw new KeyNotFoundException($"局面{id}が存在しません");
    }

    public static PlaybookNode FindNode(Playbook playbook, PlaybookNodeId id)
    {
        return playbook.Nodes.FirstOrDefault(n => n.Id == id)
            ?? throw new KeyNotFoundException($"ノード{id}が存在しません");
    }

    public static PlaybookBoardTransition FindTransition(Playbook playbook, PlaybookBoardTransitionId id)
    {
        return playbook.Transitions.FirstOrDefault(t => t.Id == id)
            ?? throw new KeyNotFoundException($"遷移{id}が存在しません");
    }

    // ノードの代表は、ノード内で最初に登録された局面とする
    public static PlaybookBoard Representative(PlaybookNode node)
    {
        return node.Boards.MinBy(b => b.Id.Value)!;
    }

    public static bool ContainsRoot(Playbook playbook, PlaybookNode node)
    {
        return node.Boards.Any(b => b.Sfen == playbook.RootSfen);
    }
}
