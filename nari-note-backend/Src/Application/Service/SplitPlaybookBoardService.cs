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
        if (node.Boards.Count <= 1) throw new InvalidOperationException("この局面だけのノードは分けられません");

        var created = new PlaybookNode { Playbook = playbook };
        node.Boards.Remove(board);
        board.Node = created;
        created.Boards.Add(board);
        playbook.Nodes.Add(created);
        this.playbookRepository.AddNode(created);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new SplitPlaybookBoardResponse();
    }
}
