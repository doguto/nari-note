using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    private readonly IArticleRepository _articleRepository;
    
    public CreateArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }
    
    public async Task<CreateArticleResponse> ExecuteAsync(CreateArticleRequest request)
    {
        var article = new Article
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null! // Will be loaded from database
        };
        
        var created = await _articleRepository.CreateAsync(article);
        
        return new CreateArticleResponse
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            AuthorId = created.AuthorId,
            Tags = request.Tags,
            IsPublished = created.IsPublished,
            CreatedAt = created.CreatedAt
        };
    }
}
