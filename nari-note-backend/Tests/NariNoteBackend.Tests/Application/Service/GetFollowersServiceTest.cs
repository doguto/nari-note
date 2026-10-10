using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NariNoteBackend.Tests.Application.Service;

public class GetFollowersServiceTest
{
    readonly IFollowRepository followRepository = Substitute.For<IFollowRepository>();
    readonly GetFollowersService service;
    readonly User user = new UserBuilder().Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public GetFollowersServiceTest()
    {
        this.service = new GetFollowersService(this.followRepository, this.userRepository);
    }

    [Fact]
    public async Task フォロワーの一覧を返す()
    {
        var follower = new UserBuilder().WithName("follower").WithProfileImage("https://images.test/follower").Build();
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.followRepository.GetFollowersAsync(this.user.Id).Returns(new List<User> { follower });

        var response = await this.service.ExecuteAsync(new GetFollowersRequest { UserId = this.user.Id });

        var dto = Assert.Single(response.Followers);
        Assert.Equal(follower.Id, dto.Id);
        Assert.Equal("follower", dto.Username);
        Assert.Equal("https://images.test/follower", dto.UserIconImageUrl);
    }

    [Fact]
    public async Task 存在しないユーザーはフォロワーを問い合わせずにエラーにする()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).ThrowsAsync(new KeyNotFoundException());
        var request = new GetFollowersRequest { UserId = this.user.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowersAsync(this.user.Id);
    }
}
