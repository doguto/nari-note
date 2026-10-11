using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetPopularTagsService
{
    readonly ITagRepository tagRepository;
    readonly TimeProvider timeProvider;

    public GetPopularTagsService(ITagRepository tagRepository, TimeProvider timeProvider)
    {
        this.tagRepository = tagRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<GetPopularTagsResponse> ExecuteAsync(GetPopularTagsRequest request)
    {
        var tags = await tagRepository.GetPopularTagsAsync(timeProvider.UtcNow().AddMonths(-3), 5);
        
        return new GetPopularTagsResponse
        {
            Tags = tags.Select(t => new TagDto
            {
                Name = t.Name,
                ArticleCount = t.ArticleTags.Count
            }).ToList()
        };
    }
}
