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
        var existing = await likeRepository.FindByUserAndArticleAsync(userId, request.ArticleId);
        bool isLiked;

        if (existing != null)
        {
            await likeRepository.DeleteAsync(existing.Id);
            isLiked = false;
        }
        else
        {
            await likeRepository.CreateAsync(
                new Like 
                { 
                    UserId = userId, 
                    ArticleId = request.ArticleId,
                }
            );
            isLiked = true;
        }

        var currentCount = await likeRepository.CountByArticleAsync(request.ArticleId);
        
        return new ToggleLikeResponse
        {
            IsLiked = isLiked,
            CurrentLikeCount = currentCount
        };
    }
}
