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

public class CreatePlaybookService
{
    readonly IPlaybookRepository playbookRepository;

    public CreatePlaybookService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task<CreatePlaybookResponse> ExecuteAsync(UserId userId, CreatePlaybookRequest request)
    {
        var start = ShogiPosition.FromSfen(string.IsNullOrWhiteSpace(request.RootSfen) ? ShogiPosition.Hirate : request.RootSfen);
        if (!start.IsValidTurn()) throw new ArgumentException("手番でない側の玉に王手がかかっている局面からは始められません");
        var rootSfen = start.ToSfen();

        var playbook = new Playbook { UserId = userId, Name = request.Name, RootSfen = rootSfen };
        var group = new ShapeGroup { Playbook = playbook };
        // 開始局面は比べる相手がいないため、候補探しを済みにしておく
        var root = new PlaybookPosition { Playbook = playbook, Sfen = rootSfen, ShapeGroup = group, CandidatesScanned = true };
        playbook.ShapeGroups.Add(group);
        playbook.Positions.Add(root);

        await this.playbookRepository.CreateAsync(playbook);
        return new CreatePlaybookResponse { Id = playbook.Id, RootPositionId = root.Id };
    }
}
