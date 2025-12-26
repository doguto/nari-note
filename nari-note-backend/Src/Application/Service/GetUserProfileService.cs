using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

/// <summary>
/// ユーザープロフィール取得サービス
/// </summary>
public class GetUserProfileService
{
    private readonly IUserRepository userRepository;
    
    public GetUserProfileService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    /// <summary>
    /// ユーザーIDでユーザープロフィールを取得する
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <returns>ユーザープロフィールレスポンス</returns>
    /// <exception cref="NotFoundException">ユーザーが見つからない場合</exception>
    public async Task<GetUserProfileResponse> ExecuteAsync(GetUserProfileRequest request)
    {
        var user = await this.userRepository.FindByIdAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.Id} not found");
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
