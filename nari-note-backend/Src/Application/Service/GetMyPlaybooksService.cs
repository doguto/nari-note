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

public class GetMyPlaybooksService
{
    readonly IPlaybookRepository playbookRepository;

    public GetMyPlaybooksService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task<GetMyPlaybooksResponse> ExecuteAsync(UserId userId)
    {
        var playbooks = await this.playbookRepository.FindSummariesByUserAsync(userId);
        return new GetMyPlaybooksResponse
        {
            Playbooks = playbooks.Select(b => new PlaybookSummaryDto
            {
                Id = b.Id,
                Name = b.Name,
                PositionCount = b.PositionCount,
                UpdatedAt = b.UpdatedAt,
            }).ToList(),
        };
    }
}
