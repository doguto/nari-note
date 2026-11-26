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

        var board = PlaybookAccess.FindBoard(playbook, request.BoardId);
        var other = PlaybookAccess.FindBoard(playbook, request.OtherBoardId);
        if (board == other || (board.Node != null && board.Node == other.Node)) return new MergePlaybookNodesResponse();

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        var keep = graph.EnsureNode(board);
        var drop = graph.EnsureNode(other);

        var memo = string.Join("\n", new[] { keep.Memo, drop.Memo }.Where(m => m.Length > 0));
        if (memo.Length > PlaybookNode.MemoMaxLength) throw new ArgumentException("まとめると研究メモが2000文字を超えます。先にメモを短くしてください");

        foreach (var member in drop.Boards.ToList())
        {
            member.Node = keep;
            keep.Boards.Add(member);
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
