using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class LocalImageStorageGateway : IImageStorageGateway
{
    public Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType)
        => Task.FromResult($"https://placeholder.example.com/users/{userId}/icon");
}
