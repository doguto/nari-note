using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class DeletePlaybookTransitionService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public DeletePlaybookTransitionService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    // 辺を消すと、開始局面から辿れなくなったノードとその先も消える
    public async Task ExecuteAsync(UserId userId, DeletePlaybookTransitionRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var transition = PlaybookAccess.FindTransition(playbook, request.TransitionId);
        var tree = PlaybookTree.Load(playbook);
        tree.RemoveMove(transition.FromBoard.Sfen, transition.Usis[0]);
        PlaybookAccess.Rebuild(playbook, tree, this.playbookRepository);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
    }
}
