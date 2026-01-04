using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateUserProfileService
{
    readonly IUserRepository userRepository;

    public UpdateUserProfileService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<UpdateUserProfileResponse> ExecuteAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await userRepository.FindForceByIdAsync(userId);

        // nullでない値のみ更新
        if (!request.Name.IsNullOrEmpty())
        {
            user.Name = request.Name;
        }

        if (request.ProfileImage != null)
        {
            user.ProfileImage = request.ProfileImage;
        }

        if (request.Bio != null)
        {
            user.Bio = request.Bio;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);

        return new UpdateUserProfileResponse
        {
            Id = user.Id,
            UpdatedAt = user.UpdatedAt
        };
    }
}
