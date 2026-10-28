using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeImageStorageGateway : IImageStorageGateway
{
    public HashSet<string> UploadedUserIds { get; } = new();
    public List<string> UploadedArticleImageUrls { get; } = new();

    public string GetUserIconUrl(string userId)
    {
        return $"https://images.test/users/{userId}/icon";
    }

    public Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType)
    {
        this.UploadedUserIds.Add(userId);
        return Task.FromResult(GetUserIconUrl(userId));
    }

    public Task DeleteUserIconAsync(string userId)
    {
        this.UploadedUserIds.Remove(userId);
        return Task.CompletedTask;
    }

    public Task<string> UploadArticleImageAsync(string userId, string imageId, Stream imageStream, string contentType)
    {
        var url = $"https://images.test/articles/images/{userId}/{imageId}";
        this.UploadedArticleImageUrls.Add(url);
        return Task.FromResult(url);
    }
}
