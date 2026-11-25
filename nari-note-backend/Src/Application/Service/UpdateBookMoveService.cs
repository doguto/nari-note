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

public class UpdateBookMoveService
{
    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public UpdateBookMoveService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdateBookMoveResponse> ExecuteAsync(UserId userId, UpdateBookMoveRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var move = book.Moves.FirstOrDefault(m => m.Id == request.MoveId)
            ?? throw new KeyNotFoundException($"指し手{request.MoveId}が存在しません");

        if (request.Memo != null) move.Memo = request.Memo;
        if (request.Condition != null)
        {
            // 条件はグラフの辺に付くので、同じ形から同じ手で同じ形へ進む指し手すべてにそろえる
            var groupOf = book.Positions.ToDictionary(p => p.Id, p => p.ShapeGroupId);
            var sameEdge = book.Moves.Where(m =>
                m.Usi == move.Usi
                && groupOf[m.ParentPositionId] == groupOf[move.ParentPositionId]
                && groupOf[m.ChildPositionId] == groupOf[move.ChildPositionId]
            );
            foreach (var m in sameEdge) m.Condition = request.Condition;
        }
        book.UpdatedAt = this.timeProvider.UtcNow();
        await this.bookRepository.SaveAsync();
        return new UpdateBookMoveResponse();
    }
}
