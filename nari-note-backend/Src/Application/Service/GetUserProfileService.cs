using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    readonly IUserRepository userRepository;
    
    public GetUserProfileService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    public async Task<GetUserProfileResponse> ExecuteAsync(GetUserProfileRequest request)
    {
        var user = await userRepository.GetByIdAsync(request.Id);

        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Name,  // Domain の Name を Username として返す
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
            // PasswordHash と Email は含めない
        };
    }
}
