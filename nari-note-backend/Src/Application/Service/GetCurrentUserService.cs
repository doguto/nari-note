using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetCurrentUserService
{
    public Task<AuthResponse> ExecuteAsync(GetCurrentUserRequest request, UserId? currentUserId)
    {
        var response = new AuthResponse
        {
            UserId = currentUserId
        };
        
        return Task.FromResult(response);
    }
}
