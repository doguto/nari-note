using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;

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
        DateTime? publishedAt = null;
        if (request.PublishedAt.HasValue)
        {
            publishedAt = request.PublishedAt.Value;
        }
        else if (request.IsPublished)
        {
            publishedAt = DateTime.UtcNow;
        }

        var article = new Article
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            PublishedAt = publishedAt
        };

        var created = await articleRepository.CreateAsync(article);

        // タグと一緒に記事を更新（1回のDB操作に統合）
        if (request.Tags.Count > 0)
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
