using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    private readonly IArticleRepository articleRepository;
    
    public CreateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
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
            Author = null! // EF Core handles navigation property via AuthorId
        };
        
        var created = await this.articleRepository.CreateAsync(article);
        
        return new CreateArticleResponse
        {
            Id = created.Id,
            CreatedAt = created.CreatedAt
        };
    }
}
