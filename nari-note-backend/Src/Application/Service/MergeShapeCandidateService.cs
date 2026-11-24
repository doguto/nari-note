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

public class MergeShapeCandidateService
{
    const int MemoMaxLength = 10000;

    readonly IBookRepository bookRepository;
    readonly TimeProvider timeProvider;

    public MergeShapeCandidateService(IBookRepository bookRepository, TimeProvider timeProvider)
    {
        this.bookRepository = bookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<MergeShapeCandidateResponse> ExecuteAsync(UserId userId, MergeShapeCandidateRequest request)
    {
        var book = await this.bookRepository.FindForceGraphByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        var candidate = book.ShapeCandidates.FirstOrDefault(c => c.Id == request.CandidateId)
            ?? throw new KeyNotFoundException($"候補{request.CandidateId}が存在しません");
        if (candidate.Status != ShapeCandidateStatus.Done) throw new InvalidOperationException("判定が終わっていない候補はまとめられません");

        var keep = book.ShapeGroups.First(g => g.Id == BookAccess.FindPosition(book, candidate.PositionAId).ShapeGroupId);
        var drop = book.ShapeGroups.First(g => g.Id == BookAccess.FindPosition(book, candidate.PositionBId).ShapeGroupId);
        if (keep.Id != drop.Id)
        {
            var memo = string.Join("\n", new[] { keep.Memo, drop.Memo }.Where(m => m.Length > 0));
            if (memo.Length > MemoMaxLength) throw new ArgumentException("まとめると研究メモが10000文字を超えます。先にメモを短くしてください");

            foreach (var p in book.Positions.Where(p => p.ShapeGroupId == drop.Id))
            {
                p.ShapeGroup = keep;
                p.ShapeGroupId = keep.Id;
            }
            keep.Memo = memo;
            if (keep.Name.Length == 0) keep.Name = drop.Name;
            this.bookRepository.RemoveShapeGroup(drop);
        }

        candidate.Decision = ShapeCandidateDecision.Merged;
        book.UpdatedAt = this.timeProvider.UtcNow();
        await this.bookRepository.SaveAsync();
        return new MergeShapeCandidateResponse();
    }
}
