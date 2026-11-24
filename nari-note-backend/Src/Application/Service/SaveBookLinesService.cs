using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

// 研究盤で手元に指し進めた手順を、保存のときにまとめて定跡へ入れる
public class SaveBookLinesService
{
    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public SaveBookLinesService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SaveBookLinesResponse> ExecuteAsync(UserId userId, SaveBookLinesRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);

        var visited = new List<BookPosition>();
        var changed = false;
        foreach (var line in request.Lines)
        {
            var position = BookAccess.FindPosition(book, line.FromPositionId);
            foreach (var usi in line.Usis)
            {
                var (child, added) = this.Play(book, position, usi);
                changed |= added;
                visited.Add(child);
                position = child;
            }
        }

        if (changed)
        {
            book.UpdatedAt = this.timeProvider.UtcNow();
            await this.bookRepository.SaveAsync();
        }

        return new SaveBookLinesResponse
        {
            Positions = visited.DistinctBy(p => p.Sfen).Select(p => new SavedPositionDto { Sfen = p.Sfen, PositionId = p.Id }).ToList(),
        };
    }

    (BookPosition Child, bool Added) Play(Book book, BookPosition parent, string usi)
    {
        var shogi = ShogiPosition.FromSfen(parent.Sfen);
        var move = ShogiMove.Parse(usi);
        if (!shogi.IsLegal(move)) throw new ArgumentException($"この局面では指せない手です: {usi}");
        var normalized = move.ToUsi();

        // 同じ保存の中で追加した手や局面はまだ Id を持たないので、参照で探す
        var existing = book.Moves.FirstOrDefault(m => m.ParentPosition == parent && m.Usi == normalized);
        if (existing != null) return (existing.ChildPosition, false);

        var childSfen = shogi.Apply(move).ToSfen();
        var child = book.Positions.FirstOrDefault(p => p.Sfen == childSfen);
        if (child == null)
        {
            var group = new ShapeGroup { Book = book };
            child = new BookPosition { Book = book, Sfen = childSfen, ShapeGroup = group };
            this.bookRepository.AddShapeGroup(group);
            this.bookRepository.AddPosition(child);
            book.ShapeGroups.Add(group);
            book.Positions.Add(child);
        }

        var bookMove = new BookMove
        {
            Book = book,
            ParentPosition = parent,
            ChildPosition = child,
            Usi = normalized,
            Label = ShogiNotation.MoveLabel(shogi, move),
        };
        this.bookRepository.AddMove(bookMove);
        book.Moves.Add(bookMove);
        return (child, true);
    }
}
