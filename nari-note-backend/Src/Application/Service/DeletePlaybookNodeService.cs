using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class DeletePlaybookNodeService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public DeletePlaybookNodeService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    // ノードに入る辺を丸ごと外し、そのノードと、その先にしか無い辺やノードを消す
    public async Task ExecuteAsync(UserId userId, DeletePlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var node = PlaybookAccess.FindNode(playbook, request.NodeId);
        if (PlaybookAccess.ContainsRoot(playbook, node)) throw new InvalidOperationException("開始局面のノードは削除できません");

        var tree = PlaybookTree.Load(playbook);
        foreach (var incoming in playbook.Transitions.Where(t => t.ToBoard?.Node == node))
        {
            tree.RemoveMove(incoming.FromBoard.Sfen, incoming.Usis[0]);
        }
        PlaybookAccess.Rebuild(playbook, tree, this.playbookRepository);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
    }
}
