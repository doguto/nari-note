using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class WithdrawService
{
    readonly IImageStorageGateway imageStorageGateway;
    readonly IUserRepository userRepository;

    public WithdrawService(
        IUserRepository userRepository,
        IImageStorageGateway imageStorageGateway
    )
    {
        this.userRepository = userRepository;
        this.imageStorageGateway = imageStorageGateway;
    }

    public async Task<WithdrawResponse> ExecuteAsync(UserId userId, WithdrawRequest request, HttpResponse response)
    {
        var user = await userRepository.FindForceByIdAsync(userId);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new ArgumentException("パスワードが正しくありません");
        }

        // 記事・講座・いいね・コメント・フォロー・通知・認証トークン等は Cascade で削除される
        await userRepository.DeleteAsync(userId);

        // 外部ストレージはロールバックできないため、DB 操作が成功した後に削除する
        if (user.ProfileImage != null)
        {
            await imageStorageGateway.DeleteUserIconAsync(userId.Value.ToString());
        }

        response.Cookies.Delete("authToken", new CookieOptions
        {
            Path = "/"
        });

        return new WithdrawResponse();
    }
}
