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

public class RemovePlaybookMoveService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public RemovePlaybookMoveService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    // 手を外すと、その先にしかない局面・遷移・ノードも消える
    public async Task<RemovePlaybookMoveResponse> ExecuteAsync(UserId userId, RemovePlaybookMoveRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        var board = graph.FindForceBoard(ShogiBoard.Normalize(request.Sfen));
        var usi = ShogiMove.Parse(request.Usi).ToUsi();
        var transition = graph.Outgoing(board).FirstOrDefault(t => t.Usi == usi)
            ?? throw new KeyNotFoundException($"指し手{usi}が存在しません");
        graph.RemoveTransition(transition);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new RemovePlaybookMoveResponse();
    }
}
