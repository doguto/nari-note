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

public class DeleteBookMoveService
{
    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public DeleteBookMoveService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(UserId userId, DeleteBookMoveRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var move = book.Moves.FirstOrDefault(m => m.Id == request.MoveId)
            ?? throw new KeyNotFoundException($"指し手{request.MoveId}が存在しません");

        this.bookRepository.RemoveMove(move);
        var remaining = book.Moves.Where(m => m.Id != move.Id).ToList();
        this.RemoveUnreachable(book, remaining);

        book.UpdatedAt = this.timeProvider.UtcNow();
        await this.bookRepository.SaveAsync();
    }

    // 外した手の先にしかない局面は、開始局面から辿れなくなるので一緒に消す
    void RemoveUnreachable(Book book, List<BookMove> remaining)
    {
        var root = BookAccess.Root(book);
        var children = remaining.ToLookup(m => m.ParentPositionId);
        var reachable = new HashSet<BookPositionId> { root.Id };
        var queue = new Queue<BookPositionId>([root.Id]);
        while (queue.Count > 0)
        {
            foreach (var m in children[queue.Dequeue()])
            {
                if (reachable.Add(m.ChildPositionId)) queue.Enqueue(m.ChildPositionId);
            }
        }

        var removed = book.Positions.Where(p => !reachable.Contains(p.Id)).ToList();
        var removedIds = removed.Select(p => p.Id).ToHashSet();
        foreach (var m in remaining.Where(m => removedIds.Contains(m.ParentPositionId) || removedIds.Contains(m.ChildPositionId)))
        {
            this.bookRepository.RemoveMove(m);
        }
        foreach (var c in book.ShapeCandidates.Where(c => removedIds.Contains(c.PositionAId) || removedIds.Contains(c.PositionBId)).ToList())
        {
            this.bookRepository.RemoveShapeCandidate(c);
        }
        foreach (var p in removed)
        {
            this.bookRepository.RemovePosition(p);
        }

        var usedGroups = book.Positions.Where(p => !removedIds.Contains(p.Id)).Select(p => p.ShapeGroupId).ToHashSet();
        foreach (var g in book.ShapeGroups.Where(g => !usedGroups.Contains(g.Id)).ToList())
        {
            this.bookRepository.RemoveShapeGroup(g);
        }
    }
}
