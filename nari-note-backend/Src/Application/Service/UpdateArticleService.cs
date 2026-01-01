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
    
    public async Task<UpdateArticleResponse> ExecuteAsync(UpdateArticleRequest request)
    {
        var article = await this.articleRepository.GetByIdAsync(request.Id);
            
        if (article.AuthorId != request.UserId)
            throw new ForbiddenException("この記事を更新する権限がありません");
        
        // Only update non-null values
        if (!string.IsNullOrEmpty(request.Title)) 
            article.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Body)) 
            article.Body = request.Body;
        if (request.IsPublished != null) 
            article.IsPublished = request.IsPublished.Value;
        
        article.UpdatedAt = DateTime.UtcNow;
        
        await this.articleRepository.UpdateAsync(article);
        
        // Update tags if provided
        if (request.Tags != null)
        {
            await this.articleRepository.UpdateArticleTagsAsync(article.Id, request.Tags);
        }
        
        // Reload article to get updated tags
        var updatedArticle = await this.articleRepository.GetByIdAsync(request.Id);
        
        return new UpdateArticleResponse
        {
            Id = updatedArticle.Id,
            Title = updatedArticle.Title,
            Body = updatedArticle.Body,
            Tags = updatedArticle.ArticleTags.Select(at => at.Tag.Name).ToList(),
            IsPublished = updatedArticle.IsPublished,
            UpdatedAt = updatedArticle.UpdatedAt
        };
    }
}
