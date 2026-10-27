using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;

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
        this.followRepository.GetFollowersAsync(this.user.Id, 20, 0).Returns((new List<User> { follower }, 1));

        var response = await this.service.ExecuteAsync(new GetFollowersRequest { UserId = this.user.Id });

        var dto = Assert.Single(response.Followers);
        Assert.Equal(follower.Id, dto.Id);
        Assert.Equal("follower", dto.Username);
        Assert.Equal("https://images.test/follower", dto.UserIconImageUrl);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task 存在しないユーザーはフォロワーを問い合わせずにエラーにする()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).ThrowsAsync(new KeyNotFoundException());
        var request = new GetFollowersRequest { UserId = this.user.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowersAsync(this.user.Id, 0, 0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(101, 0)]
    [InlineData(20, -1)]
    public async Task 範囲外の件数や位置は検索せずにエラーにする(int limit, int offset)
    {
        var request = new GetFollowersRequest { UserId = this.user.Id, Limit = limit, Offset = offset };

        await Assert.ThrowsAsync<ValidationException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowersAsync(this.user.Id, 0, 0);
    }
}
