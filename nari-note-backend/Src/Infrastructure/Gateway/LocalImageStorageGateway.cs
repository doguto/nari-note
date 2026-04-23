using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public class LocalImageStorageGateway : IImageStorageGateway
{
    public string GetUserIconUrl(string userId) => $"https://placeholder.example.com/users/{userId}/icon";

    public Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType)
        => Task.FromResult(GetUserIconUrl(userId));
}
