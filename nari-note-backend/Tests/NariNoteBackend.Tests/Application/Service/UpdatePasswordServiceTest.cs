using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UpdatePasswordServiceTest
{
    const string CurrentPassword = "password123";

    readonly UpdatePasswordService service;
    readonly User user = new UserBuilder().WithPassword(CurrentPassword).Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public UpdatePasswordServiceTest()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.service = new UpdatePasswordService(this.userRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task 現在のパスワードが正しければ新しいパスワードに更新する()
    {
        await this.service.ExecuteAsync(
            this.user.Id,
            new UpdatePasswordRequest { CurrentPassword = CurrentPassword, NewPassword = "new-password123" }
        );

        Assert.True(BCrypt.Net.BCrypt.Verify("new-password123", this.user.PasswordHash));
        Assert.Equal(TestTimeProvider.DefaultUtcNow, this.user.UpdatedAt);
        await this.userRepository.Received(1).UpdateAsync(this.user);
    }

    [Fact]
    public async Task 現在のパスワードが誤っていれば更新しない()
    {
        var originalHash = this.user.PasswordHash;
        var request = new UpdatePasswordRequest { CurrentPassword = "wrong-password", NewPassword = "new-password123" };

        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(this.user.Id, request));

        Assert.Equal(originalHash, this.user.PasswordHash);
        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }
}
