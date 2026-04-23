using Amazon.S3;
using Amazon.S3.Model;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class S3ImageStorageGateway : IImageStorageGateway
{
    readonly string bucketName;
    readonly string domain;
    readonly Lazy<AmazonS3Client> s3Client = new(() => new AmazonS3Client());

    public S3ImageStorageGateway(IConfiguration configuration)
    {
        bucketName = configuration["image_delivery_bucket_name"]!;
        domain = configuration["image_delivery_domain"]!;
    }

    public string GetUserIconUrl(string userId) => $"https://{domain}/users/{userId}/icon";

    public async Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType)
    {
        var key = $"users/{userId}/icon";

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = imageStream,
            ContentType = contentType,
        };

        await s3Client.Value.PutObjectAsync(request);

        return GetUserIconUrl(userId);
    }
}
