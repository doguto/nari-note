using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetCurrentUserService
{
    readonly IImageStorageGateway imageStorageGateway;

    public GetCurrentUserService(IImageStorageGateway imageStorageGateway)
    {
        this.imageStorageGateway = imageStorageGateway;
    }

    public AuthResponse Execute(GetCurrentUserRequest request, UserId? currentUserId, string? userName)
    {
        var profileImage = currentUserId.HasValue
            ? imageStorageGateway.GetUserIconUrl(currentUserId.Value.Value.ToString())
            : null;

        return new AuthResponse
        {
            UserId = currentUserId,
            UserName = userName,
            UserIconImageUrl = profileImage
        };
    }
}
