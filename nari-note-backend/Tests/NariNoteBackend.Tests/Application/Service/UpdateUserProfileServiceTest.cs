using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UpdateUserProfileServiceTest
{
    readonly UpdateUserProfileService service;
    readonly User user = new UserBuilder().WithName("taro").WithBio("旧自己紹介").Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public UpdateUserProfileServiceTest()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.service = new UpdateUserProfileService(this.userRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task 名前と自己紹介を更新し更新日時を現在時刻にする()
    {
        await this.service.ExecuteAsync(this.user.Id, new UpdateUserProfileRequest { Name = "jiro", Bio = "新自己紹介" });

        Assert.Equal("jiro", this.user.Name);
        Assert.Equal("新自己紹介", this.user.Bio);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, this.user.UpdatedAt);
        await this.userRepository.Received(1).UpdateAsync(this.user);
    }

    [Fact]
    public async Task 未指定の項目は変更せず_自己紹介は空文字で消去できる()
    {
        await this.service.ExecuteAsync(this.user.Id, new UpdateUserProfileRequest { Bio = "" });

        Assert.Equal("taro", this.user.Name);
        Assert.Equal("", this.user.Bio);
        // 名前を変更しない場合は重複確認を行わない
        await this.userRepository.DidNotReceiveWithAnyArgs().ExistsByNameAsync("");
    }

    [Fact]
    public async Task 他のユーザーが使用中の名前には変更できない()
    {
        this.userRepository.ExistsByNameAsync("jiro", this.user.Id).Returns(true);
        var request = new UpdateUserProfileRequest { Name = "jiro" };

        await Assert.ThrowsAsync<ConflictException>(() => this.service.ExecuteAsync(this.user.Id, request));

        Assert.Equal("taro", this.user.Name);
        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }
}
