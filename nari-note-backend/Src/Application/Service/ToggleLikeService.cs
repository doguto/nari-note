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
    
    public async Task<ToggleLikeResponse> ExecuteAsync(int userId, ToggleLikeRequest request)
    {
        var article = await this.articleRepository.GetByIdAsync(request.ArticleId);
        
        var existing = await this.likeRepository.FindByUserAndArticleAsync(userId, request.ArticleId);
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
                UserId = userId, 
                ArticleId = request.ArticleId,
                User = null!,
                Article = null!
            });
            isLiked = true;
        }
        
        var currentCount = await this.likeRepository.CountByArticleAsync(request.ArticleId);
        
        return new ToggleLikeResponse
        {
            IsLiked = isLiked,
            CurrentLikeCount = currentCount
        };
    }
}
