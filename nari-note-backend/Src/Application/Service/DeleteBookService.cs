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

public class DeleteBookService
{
    readonly IBookRepository bookRepository;

    public DeleteBookService(IBookRepository bookRepository)
    {
        this.bookRepository = bookRepository;
    }

    public async Task ExecuteAsync(UserId userId, DeleteBookRequest request)
    {
        var book = await this.bookRepository.FindForceByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        await this.bookRepository.DeleteAsync(request.Id);
    }
}
