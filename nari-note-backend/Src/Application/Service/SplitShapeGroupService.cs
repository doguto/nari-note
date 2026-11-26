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

public class SplitShapeGroupService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public SplitShapeGroupService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SplitShapeGroupResponse> ExecuteAsync(UserId userId, SplitShapeGroupRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var position = PlaybookAccess.FindPosition(playbook, request.PositionId);
        if (playbook.Positions.Count(p => p.ShapeGroupId == position.ShapeGroupId) <= 1)
            throw new InvalidOperationException("この局面だけの形は分けられません");

        var group = new ShapeGroup { Playbook = playbook };
        this.playbookRepository.AddShapeGroup(group);
        position.ShapeGroup = group;

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new SplitShapeGroupResponse();
    }
}
