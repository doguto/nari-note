using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetFollowersService
{
    readonly IFollowRepository followRepository;
    readonly IUserRepository userRepository;
    
    public GetFollowersService(IFollowRepository followRepository, IUserRepository userRepository)
    {
        this.followRepository = followRepository;
        this.userRepository = userRepository;
    }
    
    public async Task<GetFollowersResponse> ExecuteAsync(GetFollowersRequest request)
    {
        // ユーザーが存在するか確認
        await userRepository.FindForceByIdAsync(request.UserId);
        
        var followers = await followRepository.GetFollowersAsync(request.UserId);
        
        return new GetFollowersResponse
        {
            Followers = followers.Select(user => new FollowerUserDto
            {
                Id = user.Id,
                Username = user.Name,
                ProfileImage = user.ProfileImage
            }).ToList()
        };
    }
}
