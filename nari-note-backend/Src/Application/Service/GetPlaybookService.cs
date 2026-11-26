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
        var playbook = await this.playbookRepository.FindForceByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var positions = await this.playbookRepository.FindPositionsAsync(playbook.Id);
        var root = positions.First(p => p.Sfen == playbook.RootSfen);
        return new GetPlaybookResponse { Id = playbook.Id, Name = playbook.Name, RootPositionId = root.Id };
    }
}
