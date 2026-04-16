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
        var tags = await tagRepository.GetPopularTagsAsync(DateTime.UtcNow.AddMonths(-3), 5);
        
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
