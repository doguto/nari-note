using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    private readonly IUserRepository _userRepository;
    
    public GetUserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<GetUserProfileResponse?> ExecuteAsync(GetUserProfileRequest request)
    {
        var user = await _userRepository.FindByIdAsync(request.Id);
        if (user == null) return null;
        
        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Name,  // Domain の Name を Username として返す
            Email = user.Email,
            Bio = null,  // Domain に Bio フィールドがないため null
            CreatedAt = user.CreatedAt
            // PasswordHash は含めない
        };
    }
}
