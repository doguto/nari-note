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

public class SplitShapeGroupService
{
    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public SplitShapeGroupService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SplitShapeGroupResponse> ExecuteAsync(UserId userId, SplitShapeGroupRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var position = BookAccess.FindPosition(book, request.PositionId);
        if (book.Positions.Count(p => p.ShapeGroupId == position.ShapeGroupId) <= 1)
            throw new InvalidOperationException("この局面だけの形は分けられません");

        var group = new ShapeGroup { Book = book };
        this.bookRepository.AddShapeGroup(group);
        position.ShapeGroup = group;

        book.UpdatedAt = this.timeProvider.UtcNow();
        await this.bookRepository.SaveAsync();
        return new SplitShapeGroupResponse();
    }
}
