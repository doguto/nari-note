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
    readonly IPlaybookRepository playbookRepository;

    public RejectShapeCandidateService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task<RejectShapeCandidateResponse> ExecuteAsync(UserId userId, RejectShapeCandidateRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var candidate = playbook.ShapeCandidates.FirstOrDefault(c => c.Id == request.CandidateId)
            ?? throw new KeyNotFoundException($"候補{request.CandidateId}が存在しません");

        candidate.Decision = ShapeCandidateDecision.Rejected;
        await this.playbookRepository.SaveAsync();
        return new RejectShapeCandidateResponse();
    }
}
