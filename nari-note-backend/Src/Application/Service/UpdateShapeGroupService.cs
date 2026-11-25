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

public class UpdateShapeGroupService
{
    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public UpdateShapeGroupService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdateShapeGroupResponse> ExecuteAsync(UserId userId, UpdateShapeGroupRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var group = book.ShapeGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new KeyNotFoundException($"形{request.GroupId}が存在しません");

        if (request.Name != null) group.Name = request.Name;
        if (request.Memo != null) group.Memo = request.Memo;
        book.UpdatedAt = this.timeProvider.UtcNow();
        await this.bookRepository.SaveAsync();
        return new UpdateShapeGroupResponse();
    }
}
