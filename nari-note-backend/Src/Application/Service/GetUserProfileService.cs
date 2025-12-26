using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Exception;

namespace NariNoteBackend.Application.Service;

public class GetUserProfileService
{
    private readonly IUserRepository userRepository;
    
    public GetUserProfileService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    public async Task<GetUserProfileResponse> ExecuteAsync(GetUserProfileRequest request)
    {
        var user = await this.userRepository.FindByIdAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException("ユーザーが見つかりません");
        }
        
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
