using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using SkiaSharp;

namespace NariNoteBackend.Application.Service;

public class UploadUserIconService
{
    const long MaxFileSizeBytes = 5_000_000;

    static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

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
            throw new ArgumentException("対応していない画像形式です。JPEG、PNG、WebPのみ対応しています。");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("ファイルサイズは5MB以内にしてください。");
        }

        await using var stream = file.OpenReadStream();

        if (!IsValidImageMagicBytesAsync(stream))
        {
            throw new ArgumentException("ファイルの形式が正しくありません。");
        }

        stream.Seek(0, SeekOrigin.Begin);

        var iconUrl = await imageStorageGateway.UploadUserIconAsync(userId.Value.ToString(), stream, file.ContentType);

        var user = await userRepository.FindForceByIdAsync(userId);
        user.ProfileImage = iconUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        return new UploadUserIconResponse { UserIconImageUrl = iconUrl };
    }

    bool IsValidImageMagicBytesAsync(Stream stream)
    {
        try
        {
            using var codec = SKCodec.Create(stream);
            return codec != null;
        }
        catch
        {
            return false;
        }
    }
}
