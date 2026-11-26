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

    // ノードに属さない局面に名前やメモを付けるとノードを作り、名前もメモも消すとノードから外す
    public async Task<UpdatePlaybookNodeResponse> ExecuteAsync(UserId userId, UpdatePlaybookNodeRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        var board = graph.FindForceBoard(ShogiBoard.Normalize(request.Sfen));
        var node = graph.EnsureNode(board);
        if (request.Name != null) node.Name = request.Name.Trim();
        if (request.Memo != null) node.Memo = request.Memo;
        graph.ReleaseNodeIfEmpty(node);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new UpdatePlaybookNodeResponse();
    }
}
