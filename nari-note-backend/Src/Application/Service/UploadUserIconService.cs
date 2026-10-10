using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using SkiaSharp;

namespace NariNoteBackend.Application.Service;

public class UploadUserIconService
{
    const long MaxFileSizeBytes = 5_000_000;

    static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

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
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("対応していない画像形式です。JPEG、PNG、WebPのみ対応しています。");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("ファイルサイズは5MB以内にしてください。");
        }

        await using var stream = file.OpenReadStream();

        if (!IsDecodableImage(stream))
        {
            throw new ArgumentException("ファイルの形式が正しくありません。");
        }

        stream.Seek(0, SeekOrigin.Begin);

        var iconUrl = await imageStorageGateway.UploadUserIconAsync(userId.Value.ToString(), stream, file.ContentType);

        var user = await userRepository.FindForceByIdAsync(userId);
        user.ProfileImage = iconUrl;
        user.UpdatedAt = timeProvider.UtcNow();
        await userRepository.UpdateAsync(user);

        return new UploadUserIconResponse { UserIconImageUrl = iconUrl };
    }

    // SKCodec に Stream を直接渡すと codec の破棄時に Stream も破棄されるため、SKData にコピーして渡す。
    // ネイティブライブラリの読み込み失敗を形式エラーとして扱わないよう、例外は捕捉しない。
    static bool IsDecodableImage(Stream stream)
    {
        using var data = SKData.Create(stream);
        if (data == null) return false;

        using var codec = SKCodec.Create(data);
        return codec != null;
    }
}
