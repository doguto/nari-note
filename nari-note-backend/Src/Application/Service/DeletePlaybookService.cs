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

public class DeletePlaybookService
{
    readonly IPlaybookRepository playbookRepository;

    public DeletePlaybookService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task ExecuteAsync(UserId userId, DeletePlaybookRequest request)
    {
        var playbook = await this.playbookRepository.FindForceByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        await this.playbookRepository.DeleteAsync(request.Id);
    }
}
