using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetFollowingsService
{
    readonly IFollowRepository followRepository;
    readonly IUserRepository userRepository;
    
    public GetFollowingsService(IFollowRepository followRepository, IUserRepository userRepository)
    {
        this.followRepository = followRepository;
        this.userRepository = userRepository;
    }
    
    public async Task<GetFollowingsResponse> ExecuteAsync(GetFollowingsRequest request)
    {
        // ユーザーが存在するか確認
        await userRepository.FindForceByIdAsync(request.UserId);
        
        var followings = await followRepository.GetFollowingsAsync(request.UserId);
        
        return new GetFollowingsResponse
        {
            Followings = followings.Select(user => new FollowerUserDto
            {
                Id = user.Id,
                Username = user.Name,
                Bio = user.Bio
            }).ToList()
        };
    }
}
