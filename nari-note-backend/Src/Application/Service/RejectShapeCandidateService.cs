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

public class RejectShapeCandidateService
{
    readonly IBookRepository bookRepository;

    public RejectShapeCandidateService(IBookRepository bookRepository)
    {
        this.bookRepository = bookRepository;
    }

    public async Task<RejectShapeCandidateResponse> ExecuteAsync(UserId userId, RejectShapeCandidateRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var candidate = book.ShapeCandidates.FirstOrDefault(c => c.Id == request.CandidateId)
            ?? throw new KeyNotFoundException($"候補{request.CandidateId}が存在しません");

        candidate.Decision = ShapeCandidateDecision.Rejected;
        await this.bookRepository.SaveAsync();
        return new RejectShapeCandidateResponse();
    }
}
