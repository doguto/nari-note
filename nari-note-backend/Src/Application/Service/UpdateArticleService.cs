using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;
    readonly IHttpContextAccessor httpContextAccessor;
    
    public UpdateArticleService(IArticleRepository articleRepository, IHttpContextAccessor httpContextAccessor)
    {
        this.articleRepository = articleRepository;
        this.httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<UpdateArticleResponse> ExecuteAsync(UpdateArticleRequest request)
    {
        var userId = (int?)this.httpContextAccessor.HttpContext?.Items["UserId"] 
            ?? throw new UnauthorizedException("認証が必要です");
        
        var article = await this.articleRepository.GetByIdAsync(request.Id);
            
        if (article.AuthorId != userId)
            throw new ForbiddenException("この記事を更新する権限がありません");
        
        // Only update non-null values
        if (!string.IsNullOrEmpty(request.Title)) 
            article.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Body)) 
            article.Body = request.Body;
        if (request.IsPublished != null) 
            article.IsPublished = request.IsPublished.Value;
        
        article.UpdatedAt = DateTime.UtcNow;
        
        // Update article and tags in a single operation
        await this.articleRepository.UpdateAsync(article, request.Tags);
        
        return new UpdateArticleResponse
        {
            Id = article.Id,
            UpdatedAt = article.UpdatedAt
        };
    }
}
