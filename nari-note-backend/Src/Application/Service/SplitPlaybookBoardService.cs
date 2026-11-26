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

public class SplitPlaybookBoardService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public SplitPlaybookBoardService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SplitPlaybookBoardResponse> ExecuteAsync(UserId userId, SplitPlaybookBoardRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var board = PlaybookAccess.FindBoard(playbook, request.BoardId);
        var node = board.Node;
        if (node == null || node.Boards.Count <= 1) throw new InvalidOperationException("この局面だけのノードは分けられません");

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        graph.Unlink(board);
        graph.ReleaseNodeIfEmpty(node);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new SplitPlaybookBoardResponse();
    }
}
