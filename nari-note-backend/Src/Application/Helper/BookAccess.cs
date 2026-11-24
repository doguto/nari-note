using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Helper;

public static class BookAccess
{
    public static void EnsureOwner(Book book, UserId userId)
    {
        if (!book.IsOwnedBy(userId)) throw new ForbiddenException("この定跡を操作する権限がありません");
    }

    public static BookPosition FindPosition(Book book, BookPositionId id)
    {
        return book.Positions.FirstOrDefault(p => p.Id == id)
            ?? throw new KeyNotFoundException($"局面{id}が存在しません");
    }

    public static BookPosition Root(Book book)
    {
        return book.Positions.First(p => p.Sfen == book.RootSfen);
    }

    // 形グループの代表局面は、グループ内で最初に登録された局面とする
    public static Dictionary<ShapeGroupId, BookPosition> Representatives(IEnumerable<BookPosition> positions)
    {
        return positions.GroupBy(p => p.ShapeGroupId)
                        .ToDictionary(g => g.Key, g => g.MinBy(p => p.Id.Value)!);
    }
}
