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

public class GetMyBooksService
{
    readonly IBookRepository bookRepository;

    public GetMyBooksService(IBookRepository bookRepository)
    {
        this.bookRepository = bookRepository;
    }

    public async Task<GetMyBooksResponse> ExecuteAsync(UserId userId)
    {
        var books = await this.bookRepository.FindSummariesByUserAsync(userId);
        return new GetMyBooksResponse
        {
            Books = books.Select(b => new BookSummaryDto
            {
                Id = b.Id,
                Name = b.Name,
                PositionCount = b.PositionCount,
                UpdatedAt = b.UpdatedAt,
            }).ToList(),
        };
    }
}
