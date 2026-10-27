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
        this.followRepository.GetFollowingsAsync(this.user.Id, 20, 0).Returns((new List<User> { following }, 1));

        var response = await this.service.ExecuteAsync(new GetFollowingsRequest { UserId = this.user.Id });

        var dto = Assert.Single(response.Followings);
        Assert.Equal(following.Id, dto.Id);
        Assert.Equal("following", dto.Username);
        Assert.Equal("https://images.test/following", dto.UserIconImageUrl);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task 存在しないユーザーはフォロー中のユーザーを問い合わせずにエラーにする()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).ThrowsAsync(new KeyNotFoundException());
        var request = new GetFollowingsRequest { UserId = this.user.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowingsAsync(this.user.Id, 0, 0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(101, 0)]
    [InlineData(20, -1)]
    public async Task 範囲外の件数や位置は検索せずにエラーにする(int limit, int offset)
    {
        var request = new GetFollowingsRequest { UserId = this.user.Id, Limit = limit, Offset = offset };

        await Assert.ThrowsAsync<ValidationException>(() => this.service.ExecuteAsync(request));

        await this.followRepository.DidNotReceiveWithAnyArgs().GetFollowingsAsync(this.user.Id, 0, 0);
    }
}
