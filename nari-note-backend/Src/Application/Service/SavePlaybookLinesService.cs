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

public class SavePlaybookLinesService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public SavePlaybookLinesService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SavePlaybookLinesResponse> ExecuteAsync(UserId userId, SavePlaybookLinesRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        foreach (var line in request.Lines)
        {
            graph.AddLine(line.Usis);
        }

        playbook.UpdatedAt = this.timeProvider.UtcNow();
        await this.playbookRepository.SaveAsync();
        return new SavePlaybookLinesResponse();
    }
}
