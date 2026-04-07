using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    readonly IUserRepository userRepository;
    readonly IFollowRepository followRepository;
    readonly IArticleRepository articleRepository;
    readonly ILikeRepository likeRepository;
    
    public GetUserProfileService(
        IUserRepository userRepository, 
        IFollowRepository followRepository,
        IArticleRepository articleRepository,
        ILikeRepository likeRepository)
    {
        this.userRepository = userRepository;
        this.followRepository = followRepository;
        this.articleRepository = articleRepository;
        this.likeRepository = likeRepository;
    }
    
    public async Task<GetUserProfileResponse> ExecuteAsync(GetUserProfileRequest request, UserId? currentUserId = null)
    {
        var user = await userRepository.FindForceByIdAsync(request.Id);

        var followerCount = await followRepository.CountFollowersAsync(request.Id);
        var followingCount = await followRepository.CountFollowingsAsync(request.Id);

        bool isFollowing = false;
        if (currentUserId.HasValue)
        {
            var follow = await followRepository.FindByFollowerAndFollowingAsync(currentUserId.Value, request.Id);
            isFollowing = follow != null;
        }

        // 記事数を取得
        var articleCount = await articleRepository.CountByAuthorAsync(request.Id);

        // いいねした記事数を取得
        var likedArticleCount = await likeRepository.CountLikedArticlesByUserAsync(request.Id);

        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Name,  // Domain の Name を Username として返す
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            IsFollowing = isFollowing,
            ArticleCount = articleCount,
            LikedArticleCount = likedArticleCount
        };
    }
}
