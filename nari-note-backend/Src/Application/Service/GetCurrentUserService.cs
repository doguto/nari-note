using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetCurrentUserService
{
    readonly IUserRepository userRepository;

    public GetCurrentUserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<AuthResponse> ExecuteAsync(GetCurrentUserRequest request, UserId? currentUserId, string? userName)
    {
        string? profileImage = null;
        if (currentUserId.HasValue)
        {
            var user = await userRepository.FindByIdAsync(currentUserId.Value);
            profileImage = user?.ProfileImage;
        }

        return new AuthResponse
        {
            UserId = currentUserId,
            UserName = userName,
            ProfileImage = profileImage
        };
    }
}
