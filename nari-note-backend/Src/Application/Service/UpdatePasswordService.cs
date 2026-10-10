using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdatePasswordService
{
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public UpdatePasswordService(IUserRepository userRepository, TimeProvider timeProvider)
    {
        this.userRepository = userRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdatePasswordResponse> ExecuteAsync(UserId userId, UpdatePasswordRequest request)
    {
        var user = await userRepository.FindForceByIdAsync(userId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("現在のパスワードが正しくありません");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = timeProvider.UtcNow();

        await userRepository.UpdateAsync(user);

        return new UpdatePasswordResponse();
    }
}
