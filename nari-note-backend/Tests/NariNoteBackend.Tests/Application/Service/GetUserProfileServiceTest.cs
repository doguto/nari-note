using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NariNoteBackend.Tests.Application.Service;

public class GetUserProfileServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly IFollowRepository followRepository = Substitute.For<IFollowRepository>();
    readonly ILikeRepository likeRepository = Substitute.For<ILikeRepository>();
    readonly GetUserProfileService service;
    readonly User user = new UserBuilder().WithName("taro").WithBio("居飛車党です").WithProfileImage("https://images.test/taro").Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    readonly User viewer = new UserBuilder().Build();

    public GetUserProfileServiceTest()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.service = new GetUserProfileService(
            this.userRepository,
            this.followRepository,
            this.articleRepository,
            this.likeRepository
        );
    }

    [Fact]
    public async Task プロフィールと各種件数を返す()
    {
        this.followRepository.CountFollowersAsync(this.user.Id).Returns(3);
        this.followRepository.CountFollowingsAsync(this.user.Id).Returns(5);
        this.articleRepository.CountByAuthorAsync(this.user.Id).Returns(7);
        this.likeRepository.CountLikedArticlesByUserAsync(this.user.Id).Returns(11);

        var response = await this.service.ExecuteAsync(new GetUserProfileRequest { Id = this.user.Id });

        Assert.Equal(this.user.Id, response.Id);
        Assert.Equal("taro", response.Username);
        Assert.Equal("居飛車党です", response.Bio);
        Assert.Equal("https://images.test/taro", response.UserIconImageUrl);
        Assert.Equal(this.user.CreatedAt, response.CreatedAt);
        Assert.Equal(3, response.FollowerCount);
        Assert.Equal(5, response.FollowingCount);
        Assert.Equal(7, response.ArticleCount);
        Assert.Equal(11, response.LikedArticleCount);
    }

    [Fact]
    public async Task 未ログインならフォロー状態を問い合わせずフォロー中でないとして返す()
    {
        var response = await this.service.ExecuteAsync(new GetUserProfileRequest { Id = this.user.Id });

        Assert.False(response.IsFollowing);
        await this.followRepository.DidNotReceiveWithAnyArgs().FindByFollowerAndFollowingAsync(this.user.Id, this.user.Id);
    }

    [Fact]
    public async Task ログインユーザーがフォロー中かどうかを返す()
    {
        var request = new GetUserProfileRequest { Id = this.user.Id };

        Assert.False((await this.service.ExecuteAsync(request, this.viewer.Id)).IsFollowing);

        this.followRepository
            .FindByFollowerAndFollowingAsync(this.viewer.Id, this.user.Id)
            .Returns(TestEntity.Follow(this.viewer, this.user));
        Assert.True((await this.service.ExecuteAsync(request, this.viewer.Id)).IsFollowing);
    }

    [Fact]
    public async Task 存在しないユーザーはエラーにする()
    {
        var unknown = new UserBuilder().Build();
        this.userRepository.FindForceByIdAsync(unknown.Id).ThrowsAsync(new KeyNotFoundException());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => this.service.ExecuteAsync(new GetUserProfileRequest { Id = unknown.Id })
        );
    }
}
