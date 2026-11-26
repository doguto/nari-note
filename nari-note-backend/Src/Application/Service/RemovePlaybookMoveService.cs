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

    // 手を外すと、その先にしかない局面・ノード・手順も消える
    public async Task<RemovePlaybookMoveResponse> ExecuteAsync(UserId userId, RemovePlaybookMoveRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var tree = PlaybookTree.Load(playbook);
        var sfen = ShogiBoard.Normalize(request.Sfen);
        var usi = ShogiMove.Parse(request.Usi).ToUsi();
        if (!tree.RemoveMove(sfen, usi)) throw new KeyNotFoundException($"指し手{usi}が存在しません");
        PlaybookAccess.Rebuild(playbook, tree, this.playbookRepository);

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new RemovePlaybookMoveResponse();
    }
}
