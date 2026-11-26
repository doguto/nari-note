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

public class UpdatePlaybookTransitionService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public UpdatePlaybookTransitionService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdatePlaybookTransitionResponse> ExecuteAsync(UserId userId, UpdatePlaybookTransitionRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var transition = playbook.Transitions.FirstOrDefault(t => t.Id == request.TransitionId)
            ?? throw new KeyNotFoundException($"遷移{request.TransitionId}が存在しません");
        transition.Condition = request.Condition.Trim();

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookTransitionResponse();
    }
}
