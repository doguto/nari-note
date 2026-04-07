using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class ToggleLikeService
{
    readonly ILikeRepository likeRepository;
    
    public ToggleLikeService(ILikeRepository likeRepository)
    {
        this.likeRepository = likeRepository;
    }
    
    public async Task<ToggleLikeResponse> ExecuteAsync(UserId userId, ToggleLikeRequest request)
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
