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

public class UpdatePlaybookNodeService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public UpdatePlaybookNodeService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    // 手順の途中の局面に名前やメモを付けるとノードになり、名前もメモも消すと手順の途中に戻る
    public async Task<UpdatePlaybookNodeResponse> ExecuteAsync(UserId userId, UpdatePlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var tree = PlaybookTree.Load(playbook);
        var sfen = ShogiBoard.Normalize(request.Sfen);
        var board = playbook.Boards.FirstOrDefault(b => b.Sfen == sfen);
        if (board == null)
        {
            if (!tree.Contains(sfen)) throw new KeyNotFoundException("この定跡に無い局面です");
            var created = new PlaybookNode { Playbook = playbook };
            board = new PlaybookNodeBoard { Playbook = playbook, Node = created, Sfen = sfen };
            created.Boards.Add(board);
            playbook.Nodes.Add(created);
            playbook.Boards.Add(board);
            this.playbookRepository.AddNode(created);
            this.playbookRepository.AddBoard(board);
        }

        var node = board.Node;
        if (request.Name != null) node.Name = request.Name.Trim();
        if (request.Memo != null) node.Memo = request.Memo;
        PlaybookAccess.Rebuild(playbook, tree, this.playbookRepository);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookNodeResponse();
    }
}
