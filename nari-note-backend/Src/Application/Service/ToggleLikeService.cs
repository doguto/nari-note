using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class ToggleLikeService
{
    readonly ILikeRepository likeRepository;
    readonly IArticleRepository articleRepository;
    
    public ToggleLikeService(ILikeRepository likeRepository, IArticleRepository articleRepository)
    {
        this.likeRepository = likeRepository;
        this.articleRepository = articleRepository;
    }
    
    public async Task<ToggleLikeResponse> ExecuteAsync(ToggleLikeRequest request)
    {
        var article = await this.articleRepository.FindByIdAsync(request.ArticleId)
            ?? throw new NotFoundException($"記事ID {request.ArticleId} が見つかりません");
        
        var existing = await this.likeRepository.FindByUserAndArticleAsync(request.UserId, request.ArticleId);
        bool isLiked;
        
        if (existing != null)
        {
            await this.likeRepository.DeleteAsync(existing.Id);
            isLiked = false;
        }
        else
        {
            await this.likeRepository.CreateAsync(new Like 
            { 
                UserId = request.UserId, 
                ArticleId = request.ArticleId,
                User = null!, // EF Core handles navigation property via UserId
                Article = null! // EF Core handles navigation property via ArticleId
            });
            isLiked = true;
        }
        
        var currentCount = await this.likeRepository.CountByArticleAsync(request.ArticleId);
        
        return new ToggleLikeResponse
        {
            IsLiked = isLiked,
            ArticleId = request.ArticleId,
            UserId = request.UserId,
            CurrentLikeCount = currentCount
        };
    }
}
