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

public class DeletePlaybookMoveService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public DeletePlaybookMoveService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(UserId userId, DeletePlaybookMoveRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var move = playbook.Moves.FirstOrDefault(m => m.Id == request.MoveId)
            ?? throw new KeyNotFoundException($"指し手{request.MoveId}が存在しません");

        this.playbookRepository.RemoveMove(move);
        var remaining = playbook.Moves.Where(m => m.Id != move.Id).ToList();
        this.RemoveUnreachable(playbook, remaining);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
    }

    // 外した手の先にしかない局面は、開始局面から辿れなくなるので一緒に消す
    void RemoveUnreachable(Playbook playbook, List<PlaybookMove> remaining)
    {
        var root = PlaybookAccess.Root(playbook);
        var children = remaining.ToLookup(m => m.ParentPositionId);
        var reachable = new HashSet<PlaybookPositionId> { root.Id };
        var queue = new Queue<PlaybookPositionId>([root.Id]);
        while (queue.Count > 0)
        {
            foreach (var m in children[queue.Dequeue()])
            {
                if (reachable.Add(m.ChildPositionId)) queue.Enqueue(m.ChildPositionId);
            }
        }

        var removed = playbook.Positions.Where(p => !reachable.Contains(p.Id)).ToList();
        var removedIds = removed.Select(p => p.Id).ToHashSet();
        foreach (var m in remaining.Where(m => removedIds.Contains(m.ParentPositionId) || removedIds.Contains(m.ChildPositionId)))
        {
            this.playbookRepository.RemoveMove(m);
        }
        foreach (var c in playbook.ShapeCandidates.Where(c => removedIds.Contains(c.PositionAId) || removedIds.Contains(c.PositionBId)).ToList())
        {
            this.playbookRepository.RemoveShapeCandidate(c);
        }
        foreach (var p in removed)
        {
            this.playbookRepository.RemovePosition(p);
        }

        var usedGroups = playbook.Positions.Where(p => !removedIds.Contains(p.Id)).Select(p => p.ShapeGroupId).ToHashSet();
        foreach (var g in playbook.ShapeGroups.Where(g => !usedGroups.Contains(g.Id)).ToList())
        {
            this.playbookRepository.RemoveShapeGroup(g);
        }
    }
}
