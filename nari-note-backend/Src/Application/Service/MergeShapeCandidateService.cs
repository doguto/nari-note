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

    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public MergeShapeCandidateService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<MergeShapeCandidateResponse> ExecuteAsync(UserId userId, MergeShapeCandidateRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var candidate = playbook.ShapeCandidates.FirstOrDefault(c => c.Id == request.CandidateId)
            ?? throw new KeyNotFoundException($"候補{request.CandidateId}が存在しません");
        if (candidate.Status != ShapeCandidateStatus.Done) throw new InvalidOperationException("判定が終わっていない候補はまとめられません");

        var keep = playbook.ShapeGroups.First(g => g.Id == PlaybookAccess.FindPosition(playbook, candidate.PositionAId).ShapeGroupId);
        var drop = playbook.ShapeGroups.First(g => g.Id == PlaybookAccess.FindPosition(playbook, candidate.PositionBId).ShapeGroupId);
        if (keep.Id != drop.Id)
        {
            var memo = string.Join("\n", new[] { keep.Memo, drop.Memo }.Where(m => m.Length > 0));
            if (memo.Length > MemoMaxLength) throw new ArgumentException("まとめると研究メモが10000文字を超えます。先にメモを短くしてください");

            foreach (var p in playbook.Positions.Where(p => p.ShapeGroupId == drop.Id))
            {
                p.ShapeGroup = keep;
                p.ShapeGroupId = keep.Id;
            }
            keep.Memo = memo;
            if (keep.Name.Length == 0) keep.Name = drop.Name;
            this.playbookRepository.RemoveShapeGroup(drop);
        }

        candidate.Decision = ShapeCandidateDecision.Merged;
        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new MergeShapeCandidateResponse();
    }
}
