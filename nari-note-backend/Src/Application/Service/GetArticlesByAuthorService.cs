using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByAuthorService
{
    private readonly IArticleRepository _articleRepository;
    
    public GetArticlesByAuthorService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }
    
    public async Task<GetArticlesByAuthorResponse> ExecuteAsync(GetArticlesByAuthorRequest request)
    {
        var articles = await _articleRepository.FindByAuthorAsync(request.AuthorId);
        
        return new GetArticlesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = articles.FirstOrDefault()?.Author?.Name ?? "",
            Articles = articles.Select(a => new ArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                AuthorId = a.AuthorId,
                AuthorName = a.Author?.Name ?? "",
                IsPublished = a.IsPublished,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList(),
            TotalCount = articles.Count
        };
    }
}
