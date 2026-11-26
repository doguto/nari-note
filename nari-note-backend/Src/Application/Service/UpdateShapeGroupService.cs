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

public class UpdateShapeGroupService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public UpdateShapeGroupService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdateShapeGroupResponse> ExecuteAsync(UserId userId, UpdateShapeGroupRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var group = playbook.ShapeGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new KeyNotFoundException($"形{request.GroupId}が存在しません");

        if (request.Name != null) group.Name = request.Name;
        if (request.Memo != null) group.Memo = request.Memo;
        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdateShapeGroupResponse();
    }
}
