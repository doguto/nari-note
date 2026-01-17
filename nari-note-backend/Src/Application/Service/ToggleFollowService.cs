using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class ToggleFollowService
{
    readonly IFollowRepository followRepository;
    
    public ToggleFollowService(IFollowRepository followRepository)
    {
        this.followRepository = followRepository;
    }
    
    public async Task<ToggleFollowResponse> ExecuteAsync(UserId userId, ToggleFollowRequest request)
    {
        // 自分自身をフォローすることはできない
        if (userId == request.FollowingId)
        {
            throw new InvalidOperationException("自分自身をフォローすることはできません");
        }

        var existing = await followRepository.FindByFollowerAndFollowingAsync(userId, request.FollowingId);
        bool isFollowing;

        if (existing != null)
        {
            await followRepository.DeleteAsync(existing.Id);
            isFollowing = false;
        }
        else
        {
            await followRepository.CreateAsync(
                new Follow 
                { 
                    FollowerId = userId, 
                    FollowingId = request.FollowingId,
                }
            );
            isFollowing = true;
        }

        var currentCount = await followRepository.CountFollowersAsync(request.FollowingId);
        
        return new ToggleFollowResponse
        {
            IsFollowing = isFollowing,
            CurrentFollowerCount = currentCount
        };
    }
}
