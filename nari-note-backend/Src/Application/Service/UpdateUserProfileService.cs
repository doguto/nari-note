using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateUserProfileService
{
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public UpdateUserProfileService(IUserRepository userRepository, TimeProvider timeProvider)
    {
        this.userRepository = userRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdateUserProfileResponse> ExecuteAsync(UserId userId, UpdateUserProfileRequest request)
    {
        var user = await userRepository.FindForceByIdAsync(userId);

        // nullでない値のみ更新
        // ProfileImageとBioは空文字列も許可（クリア操作として扱う）
        if (!request.Name.IsNullOrEmpty())
        {
            if (await userRepository.ExistsByNameAsync(request.Name, userId))
            {
                throw new ConflictException("このユーザー名は既に使用されています");
            }

            user.Name = request.Name;
        }

        if (request.Bio != null)
        {
            user.Bio = request.Bio;
        }

        user.UpdatedAt = timeProvider.UtcNow();

        await userRepository.UpdateAsync(user);

        return new UpdateUserProfileResponse();
    }
}
