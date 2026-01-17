using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    readonly IUserRepository userRepository;
    readonly IFollowRepository followRepository;
    
    public GetUserProfileService(IUserRepository userRepository, IFollowRepository followRepository)
    {
        this.userRepository = userRepository;
        this.followRepository = followRepository;
    }
    
    public async Task<GetUserProfileResponse> ExecuteAsync(GetUserProfileRequest request, UserId? currentUserId = null)
    {
        var user = await userRepository.FindForceByIdAsync(request.Id);

        var followerCount = await followRepository.CountFollowersAsync(request.Id);
        var followingCount = await followRepository.CountFollowingsAsync(request.Id);

        bool? isFollowing = null;
        if (currentUserId.HasValue)
        {
            var follow = await followRepository.FindByFollowerAndFollowingAsync(currentUserId.Value, request.Id);
            isFollowing = follow != null;
        }

        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Name,  // Domain の Name を Username として返す
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            IsFollowing = isFollowing
        };
    }
}
