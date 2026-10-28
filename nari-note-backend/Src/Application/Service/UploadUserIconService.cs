using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UploadUserIconService
{
    readonly IImageStorageGateway imageStorageGateway;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public UploadUserIconService(
        IImageStorageGateway imageStorageGateway,
        IUserRepository userRepository,
        TimeProvider timeProvider
    )
    {
        this.imageStorageGateway = imageStorageGateway;
        this.userRepository = userRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UploadUserIconResponse> ExecuteAsync(UserId userId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        ImageFileValidator.Validate(file, stream);

        var iconUrl = await imageStorageGateway.UploadUserIconAsync(userId.Value.ToString(), stream, file.ContentType);

        var user = await userRepository.FindForceByIdAsync(userId);
        user.ProfileImage = iconUrl;
        user.UpdatedAt = timeProvider.UtcNow();
        await userRepository.UpdateAsync(user);

        return new UploadUserIconResponse { UserIconImageUrl = iconUrl };
    }
}
