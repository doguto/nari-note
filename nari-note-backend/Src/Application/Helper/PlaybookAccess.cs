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

    public static PlaybookPosition FindPosition(Playbook playbook, PlaybookPositionId id)
    {
        return playbook.Positions.FirstOrDefault(p => p.Id == id)
            ?? throw new KeyNotFoundException($"局面{id}が存在しません");
    }

    public static PlaybookPosition Root(Playbook playbook)
    {
        return playbook.Positions.First(p => p.Sfen == playbook.RootSfen);
    }

    // 形グループの代表局面は、グループ内で最初に登録された局面とする
    public static Dictionary<ShapeGroupId, PlaybookPosition> Representatives(IEnumerable<PlaybookPosition> positions)
    {
        return positions.GroupBy(p => p.ShapeGroupId)
                        .ToDictionary(g => g.Key, g => g.MinBy(p => p.Id.Value)!);
    }
}
