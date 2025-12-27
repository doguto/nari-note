using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByAuthorService
{
    readonly IArticleRepository articleRepository;
    
    public GetArticlesByAuthorService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<GetArticlesByAuthorResponse> ExecuteAsync(GetArticlesByAuthorRequest request)
    {
        var articles = await articleRepository.FindByAuthorAsync(request.AuthorId);
        
        var articleDtos = articles.Select(a => new ArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            IsPublished = a.IsPublished,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();
        
        return new GetArticlesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = articles.FirstOrDefault()?.Author.Name ?? "",
            Articles = articleDtos,
            TotalCount = articles.Count
        };
    }
}
