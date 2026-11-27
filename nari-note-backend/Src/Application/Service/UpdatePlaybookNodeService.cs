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

    // 辺の途中の局面ならノードにする。名前もメモも消したノードは、解除できれば手順の途中に戻す
    public async Task<UpdatePlaybookNodeResponse> ExecuteAsync(UserId userId, UpdatePlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var tree = PlaybookTree.Load(playbook);
        var sfen = ShogiBoard.Normalize(request.Sfen);
        var board = playbook.Boards.FirstOrDefault(b => b.Sfen == sfen);
        var isCreated = board == null;
        if (board == null)
        {
            if (!tree.Contains(sfen)) throw new KeyNotFoundException("この定跡に無い局面です");
            board = PlaybookAccess.AddNode(playbook, sfen, this.playbookRepository);
        }

        var node = board.Node;
        if (request.Name != null) node.Name = request.Name.Trim();
        if (request.Memo != null) node.Memo = request.Memo;

        var isCleared = (request.Name != null || request.Memo != null) && node.Name.Length == 0 && node.Memo.Length == 0;
        if (!isCreated && isCleared && CanUnlink(playbook, node, tree))
        {
            PlaybookAccess.Unlink(playbook, node, tree, this.playbookRepository);
        }
        else
        {
            PlaybookAccess.Rebuild(playbook, tree, this.playbookRepository);
        }

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookNodeResponse();
    }

    // 名前とメモを空にしただけでは、分岐点・合流点・まとめたノードは残す
    static bool CanUnlink(Playbook playbook, PlaybookNode node, PlaybookTree tree)
    {
        if (node.Boards.Count != 1 || PlaybookAccess.ContainsRoot(playbook, node)) return false;
        var sfen = node.Boards[0].Sfen;
        return tree.InDegreeOf(sfen) <= 1 && tree.ChildrenOf(sfen).Count <= 1;
    }
}
