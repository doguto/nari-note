using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

public class FakeImageStorageGateway : IImageStorageGateway
{
    public HashSet<string> UploadedUserIds { get; } = new();

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
}
