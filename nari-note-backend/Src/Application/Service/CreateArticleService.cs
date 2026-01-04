using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    readonly IArticleRepository articleRepository;
    
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
            IsPublished = request.IsPublished
        };
        
        var created = await articleRepository.CreateAsync(article);
        
        return new CreateArticleResponse
        {
            Id = created.Id,
            CreatedAt = created.CreatedAt
        };
    }
}
