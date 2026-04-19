namespace NariNoteBackend.Domain.Gateway;

public interface IImageStorageGateway
{
    Task<string> UploadUserIconAsync(string userId, Stream imageStream, string contentType);
}
