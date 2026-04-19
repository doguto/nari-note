using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class UploadUserIconService
{
    static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    readonly IImageStorageGateway imageStorageGateway;
    readonly IUserRepository userRepository;

    public UploadUserIconService(IImageStorageGateway imageStorageGateway, IUserRepository userRepository)
    {
        this.imageStorageGateway = imageStorageGateway;
        this.userRepository = userRepository;
    }

    public async Task<UploadUserIconResponse> ExecuteAsync(UserId userId, IFormFile file)
    {
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("対応していない画像形式です。JPEG、PNG、WebP、GIFのみ対応しています。");
        }

        using var stream = file.OpenReadStream();
        var iconUrl = await imageStorageGateway.UploadUserIconAsync(userId.Value.ToString(), stream, file.ContentType);

        var user = await userRepository.FindForceByIdAsync(userId);
        user.ProfileImage = iconUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        return new UploadUserIconResponse { UserIconImageUrl = iconUrl };
    }
}
