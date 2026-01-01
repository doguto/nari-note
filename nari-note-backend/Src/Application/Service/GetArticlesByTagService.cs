using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByTagService
{
    readonly IArticleRepository articleRepository;
    
    public GetArticlesByTagService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<GetArticlesByTagResponse> ExecuteAsync(GetArticlesByTagRequest request)
    {
        var articles = await this.articleRepository.FindByTagAsync(request.TagName);
        
        return new GetArticlesByTagResponse
        {
            Articles = articles.Select(a => new ArticleDto 
            { 
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                AuthorId = a.AuthorId,
                AuthorName = a.Author?.Name ?? "",
                Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
                LikeCount = a.Likes.Count,
                IsPublished = a.IsPublished,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList()
        };
    }
}
