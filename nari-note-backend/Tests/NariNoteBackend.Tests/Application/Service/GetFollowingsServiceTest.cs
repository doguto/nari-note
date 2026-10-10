using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NariNoteBackend.Tests.Application.Service;

public class GetFollowingsServiceTest
{
    readonly IFollowRepository followRepository = Substitute.For<IFollowRepository>();
    readonly GetFollowingsService service;
    readonly User user = new UserBuilder().Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public GetFollowingsServiceTest()
    {
        this.service = new GetFollowingsService(this.followRepository, this.userRepository);
    }

    [Fact]
    public async Task フォロー中のユーザーの一覧を返す()
    {
        var following = new UserBuilder().WithName("following").WithProfileImage("https://images.test/following").Build();
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.followRepository.GetFollowingsAsync(this.user.Id).Returns(new List<User> { following });

        var response = await this.service.ExecuteAsync(new GetFollowingsRequest { UserId = this.user.Id });

        var dto = Assert.Single(response.Followings);
        Assert.Equal(following.Id, dto.Id);
        Assert.Equal("following", dto.Username);
        Assert.Equal("https://images.test/following", dto.UserIconImageUrl);
    }

    [Fact]
    public async Task 存在しないユーザーはフォロー中のユーザーを問い合わせずにエラーにする()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).ThrowsAsync(new KeyNotFoundException());
        var request = new GetFollowingsRequest { UserId = this.user.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowingsAsync(this.user.Id);
    }
}
