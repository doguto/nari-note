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

public class CreateBookService
{
    readonly IBookRepository bookRepository;

    public CreateBookService(IBookRepository bookRepository)
    {
        this.bookRepository = bookRepository;
    }

    public async Task<CreateBookResponse> ExecuteAsync(UserId userId, CreateBookRequest request)
    {
        var start = ShogiPosition.FromSfen(string.IsNullOrWhiteSpace(request.RootSfen) ? ShogiPosition.Hirate : request.RootSfen);
        if (!start.IsValidTurn()) throw new ArgumentException("手番でない側の玉に王手がかかっている局面からは始められません");
        var rootSfen = start.ToSfen();

        var book = new Book { UserId = userId, Name = request.Name, RootSfen = rootSfen };
        var group = new ShapeGroup { Book = book };
        // 開始局面は比べる相手がいないため、候補探しを済みにしておく
        var root = new BookPosition { Book = book, Sfen = rootSfen, ShapeGroup = group, CandidatesScanned = true };
        book.ShapeGroups.Add(group);
        book.Positions.Add(root);

        await this.bookRepository.CreateAsync(book);
        return new CreateBookResponse { Id = book.Id, RootPositionId = root.Id };
    }
}
