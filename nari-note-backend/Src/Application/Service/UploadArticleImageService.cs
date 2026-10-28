using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class UploadArticleImageService
{
    readonly IImageStorageGateway imageStorageGateway;
    readonly TimeProvider timeProvider;

    public UploadArticleImageService(IImageStorageGateway imageStorageGateway, TimeProvider timeProvider)
    {
        this.imageStorageGateway = imageStorageGateway;
        this.timeProvider = timeProvider;
    }

    public async Task<UploadArticleImageResponse> ExecuteAsync(UserId userId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        ImageFileValidator.Validate(file, stream);

        var imageId = Guid.CreateVersion7(this.timeProvider.GetUtcNow()).ToString();
        var imageUrl = await this.imageStorageGateway.UploadArticleImageAsync(
            userId.Value.ToString(),
            imageId,
            stream,
            file.ContentType
        );

        return new UploadArticleImageResponse { ImageUrl = imageUrl };
    }
}
