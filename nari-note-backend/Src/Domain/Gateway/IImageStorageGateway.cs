namespace NariNoteBackend.Domain.Gateway;

public interface IImageStorageGateway
{
    string GetUserIconUrl(string userId);
    Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType);
    Task DeleteUserIconAsync(string userId);
}
