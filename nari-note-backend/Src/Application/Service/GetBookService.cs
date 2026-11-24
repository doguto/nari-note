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

public class GetBookService
{
    readonly IBookRepository bookRepository;

    public GetBookService(IBookRepository bookRepository)
    {
        this.bookRepository = bookRepository;
    }

    public async Task<GetBookResponse> ExecuteAsync(UserId userId, GetBookRequest request)
    {
        var book = await this.bookRepository.FindForceByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var positions = await this.bookRepository.FindPositionsAsync(book.Id);
        var root = positions.First(p => p.Sfen == book.RootSfen);
        return new GetBookResponse { Id = book.Id, Name = book.Name, RootPositionId = root.Id };
    }
}
