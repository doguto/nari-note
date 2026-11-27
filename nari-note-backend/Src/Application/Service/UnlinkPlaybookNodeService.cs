using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UnlinkPlaybookNodeService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public UnlinkPlaybookNodeService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UnlinkPlaybookNodeResponse> ExecuteAsync(UserId userId, UnlinkPlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var node = PlaybookAccess.FindNode(playbook, request.NodeId);
        PlaybookAccess.Unlink(playbook, node, PlaybookTree.Load(playbook), this.playbookRepository);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UnlinkPlaybookNodeResponse();
    }
}
