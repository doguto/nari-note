using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;
    
    public UpdateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<UpdateArticleResponse> ExecuteAsync(int userId, UpdateArticleRequest request)
    {
        var article = await articleRepository.GetByIdAsync(request.Id);
        if (article.AuthorId != userId) throw new ForbiddenException("この記事を更新する権限がありません");

        // Only update non-null values
        if (!string.IsNullOrEmpty(request.Title)) 
            article.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Body)) 
            article.Body = request.Body;
        if (request.IsPublished != null) 
            article.IsPublished = request.IsPublished.Value;

        article.UpdatedAt = DateTime.UtcNow;
        await articleRepository.UpdateAsync(article);

        // Update tags if provided
        List<string> updatedTags;
        if (request.Tags != null)
        {
            await articleRepository.UpdateArticleTagsAsync(article.Id, request.Tags);
            updatedTags = request.Tags;
        }
        else
        {
            updatedTags = article.ArticleTags.Select(at => at.Tag.Name).ToList();
        }
        
        return new UpdateArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Body = article.Body,
            Tags = updatedTags,
            IsPublished = article.IsPublished,
            UpdatedAt = article.UpdatedAt
        };
    }
}
