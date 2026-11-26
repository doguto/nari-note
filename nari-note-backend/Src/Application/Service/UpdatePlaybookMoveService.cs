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

public class UpdatePlaybookMoveService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public UpdatePlaybookMoveService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdatePlaybookMoveResponse> ExecuteAsync(UserId userId, UpdatePlaybookMoveRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var move = playbook.Moves.FirstOrDefault(m => m.Id == request.MoveId)
            ?? throw new KeyNotFoundException($"指し手{request.MoveId}が存在しません");

        if (request.Memo != null) move.Memo = request.Memo;
        if (request.Condition != null)
        {
            // 条件はグラフの辺に付くので、同じ形から同じ手で同じ形へ進む指し手すべてにそろえる
            var groupOf = playbook.Positions.ToDictionary(p => p.Id, p => p.ShapeGroupId);
            var sameEdge = playbook.Moves.Where(m =>
                m.Usi == move.Usi
                && groupOf[m.ParentPositionId] == groupOf[move.ParentPositionId]
                && groupOf[m.ChildPositionId] == groupOf[move.ChildPositionId]
            );
            foreach (var m in sameEdge) m.Condition = request.Condition;
        }
        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookMoveResponse();
    }
}
