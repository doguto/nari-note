using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetTagsService
{
    readonly ITagRepository tagRepository;

    public GetTagsService(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
    }

    public async Task<GetTagsResponse> ExecuteAsync(GetTagsRequest request)
    {
        var tags = await tagRepository.GetAllWithArticleCountAsync();

        return new GetTagsResponse
        {
            Tags = tags.Select(t => new TagDto
            {
                Name = t.Name,
                ArticleCount = t.ArticleTags.Count
            }).ToList()
        };
    }
}
