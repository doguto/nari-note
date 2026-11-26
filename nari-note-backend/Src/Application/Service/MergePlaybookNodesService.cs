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

public class MergePlaybookNodesService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public MergePlaybookNodesService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<MergePlaybookNodesResponse> ExecuteAsync(UserId userId, MergePlaybookNodesRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var keep = PlaybookAccess.FindBoard(playbook, request.BoardId).Node;
        var drop = PlaybookAccess.FindBoard(playbook, request.OtherBoardId).Node;
        if (keep == drop) return new MergePlaybookNodesResponse();

        var memo = string.Join("\n", new[] { keep.Memo, drop.Memo }.Where(m => m.Length > 0));
        if (memo.Length > PlaybookNode.MemoMaxLength) throw new ArgumentException("まとめると研究メモが10000文字を超えます。先にメモを短くしてください");

        foreach (var board in drop.Boards.ToList())
        {
            board.Node = keep;
            board.NodeId = keep.Id;
            keep.Boards.Add(board);
        }
        drop.Boards.Clear();
        keep.Memo = memo;
        if (keep.Name.Length == 0) keep.Name = drop.Name;
        playbook.Nodes.Remove(drop);
        this.playbookRepository.RemoveNode(drop);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new MergePlaybookNodesResponse();
    }
}
