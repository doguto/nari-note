using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class ToggleFollowServiceTest
{
    readonly IFollowRepository followRepository = Substitute.For<IFollowRepository>();
    readonly ToggleFollowService service;
    readonly User target = new UserBuilder().Build();
    readonly User user = new UserBuilder().Build();

    public ToggleFollowServiceTest()
    {
        this.service = new ToggleFollowService(this.followRepository);
    }

    [Fact]
    public async Task 未フォローならフォローする()
    {
        this.followRepository.CountFollowersAsync(this.target.Id).Returns(3);

        var response = await this.service.ExecuteAsync(
            this.user.Id,
            new ToggleFollowRequest { FollowingId = this.target.Id }
        );

        Assert.True(response.IsFollowing);
        Assert.Equal(3, response.CurrentFollowerCount);
        await this.followRepository.Received(1).CreateAsync(
            Arg.Is<Follow>(f => f.FollowerId == this.user.Id && f.FollowingId == this.target.Id)
        );
    }

    [Fact]
    public async Task フォロー済みならフォローを解除する()
    {
        var existing = TestEntity.Follow(this.user, this.target);
        existing.Id = FollowId.From(10);
        this.followRepository.FindByFollowerAndFollowingAsync(this.user.Id, this.target.Id).Returns(existing);

        var response = await this.service.ExecuteAsync(
            this.user.Id,
            new ToggleFollowRequest { FollowingId = this.target.Id }
        );

        Assert.False(response.IsFollowing);
        await this.followRepository.Received(1).DeleteAsync(existing.Id);
        await this.followRepository.DidNotReceive().CreateAsync(Arg.Any<Follow>());
    }

    [Fact]
    public async Task 自分自身はフォローできない()
    {
        var request = new ToggleFollowRequest { FollowingId = this.user.Id };

        await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.ExecuteAsync(this.user.Id, request));

        await this.followRepository.DidNotReceive().CreateAsync(Arg.Any<Follow>());
    }
}
