using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

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
            IsPublished = request.IsPublished,
            PublishedAt = request.PublishedAt ?? (request.IsPublished ? DateTime.UtcNow : null)
        };

        var created = await articleRepository.CreateAsync(article);

        // タグと一緒に記事を更新（1回のDB操作に統合）
        if (request.Tags != null && request.Tags.Count > 0)
        {
            await articleRepository.UpdateWithTagAsync(created, request.Tags);
        }

        return new CreateArticleResponse
        {
            Id = created.Id,
            CreatedAt = created.CreatedAt
        };
    }
}
