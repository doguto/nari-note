using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetPopularTagsService
{
    readonly ITagRepository tagRepository;

    public GetPopularTagsService(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
    }

    public async Task<GetPopularTagsResponse> ExecuteAsync(GetPopularTagsRequest request)
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
        
        var tags = await tagRepository.GetPopularTagsAsync(oneMonthAgo, 5);
        
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
