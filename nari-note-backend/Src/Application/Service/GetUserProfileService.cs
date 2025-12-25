using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    readonly IUserRepository userRepository;
    
    public GetUserProfileService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    public async Task<object> ExecuteAsync(GetUserProfileRequest request)
    {
        if (request == null)
        {
            return new GetUserProfileBadRequestResponse();
        }
        
        var user = await this.userRepository.FindByIdAsync(request.Id);
        if (user == null)
        {
            return new GetUserProfileNotFoundResponse();
        }
        
        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Name,  // Domain の Name を Username として返す
            Bio = null,  // Domain に Bio フィールドがないため null
            CreatedAt = user.CreatedAt
            // PasswordHash と Email は含めない
        };
    }
}
