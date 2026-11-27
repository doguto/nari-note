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

    // ノードでない局面ならノードにする。名前もメモも消したノードは、まとめた局面が無く解除できれば手順の途中に戻す
    public async Task<UpdatePlaybookNodeResponse> ExecuteAsync(UserId userId, UpdatePlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        var board = graph.FindForceBoard(ShogiBoard.Normalize(request.Sfen));
        var isCreated = board.Node == null;
        var node = graph.EnsureNode(board);
        if (request.Name != null) node.Name = request.Name.Trim();
        if (request.Memo != null) node.Memo = request.Memo;

        var isCleared = (request.Name != null || request.Memo != null) && node.Name.Length == 0 && node.Memo.Length == 0;
        if (!isCreated && isCleared && node.Boards.Count == 1 && graph.CanUnlink(node))
        {
            graph.Unlink(node);
        }

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookNodeResponse();
    }
}
