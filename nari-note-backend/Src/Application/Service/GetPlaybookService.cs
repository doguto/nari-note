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

public class GetPlaybookService
{
    readonly IPlaybookRepository playbookRepository;

    public GetPlaybookService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task<GetPlaybookResponse> ExecuteAsync(UserId userId, GetPlaybookRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        return new GetPlaybookResponse
        {
            Id = playbook.Id,
            Name = playbook.Name,
            RootSfen = playbook.RootSfen,
            Nodes = playbook.Nodes
                .OrderBy(n => n.Id.Value)
                .Select(n => new PlaybookNodeDto { Id = n.Id, Name = n.Name, Memo = n.Memo })
                .ToList(),
            Boards = playbook.Boards
                .OrderBy(b => b.Id.Value)
                .Select(b => new PlaybookBoardDto { Id = b.Id, NodeId = b.NodeId, Sfen = b.Sfen })
                .ToList(),
            Transitions = playbook.Transitions
                .OrderBy(t => t.Id.Value)
                .Select(t => new PlaybookBoardTransitionDto
                {
                    Id = t.Id,
                    FromBoardId = t.FromBoardId,
                    ToBoardId = t.ToBoardId,
                    Usi = t.Usi,
                    Condition = t.Condition,
                })
                .ToList(),
        };
    }
}
