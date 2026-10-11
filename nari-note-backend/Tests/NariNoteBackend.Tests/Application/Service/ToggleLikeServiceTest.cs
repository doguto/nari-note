using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class ToggleLikeServiceTest
{
    readonly Article article = new ArticleBuilder(new UserBuilder().Build()).Build();
    readonly ILikeRepository likeRepository = Substitute.For<ILikeRepository>();
    readonly ToggleLikeService service;
    readonly User user = new UserBuilder().Build();

    public ToggleLikeServiceTest()
    {
        this.service = new ToggleLikeService(this.likeRepository);
    }

    [Fact]
    public async Task 未いいねならいいねする()
    {
        this.likeRepository.CountByArticleAsync(this.article.Id).Returns(3);

        var response = await this.service.ExecuteAsync(
            this.user.Id,
            new ToggleLikeRequest { ArticleId = this.article.Id }
        );

        Assert.True(response.IsLiked);
        Assert.Equal(3, response.CurrentLikeCount);
        await this.likeRepository.Received(1).CreateAsync(
            Arg.Is<Like>(l => l.UserId == this.user.Id && l.ArticleId == this.article.Id)
        );
    }

    [Fact]
    public async Task いいね済みならいいねを解除する()
    {
        var existing = TestEntity.Like(this.user, this.article);
        existing.Id = LikeId.From(10);
        this.likeRepository.FindByUserAndArticleAsync(this.user.Id, this.article.Id).Returns(existing);
        this.likeRepository.CountByArticleAsync(this.article.Id).Returns(2);

        var response = await this.service.ExecuteAsync(
            this.user.Id,
            new ToggleLikeRequest { ArticleId = this.article.Id }
        );

        Assert.False(response.IsLiked);
        Assert.Equal(2, response.CurrentLikeCount);
        await this.likeRepository.Received(1).DeleteAsync(existing.Id);
        await this.likeRepository.DidNotReceive().CreateAsync(Arg.Any<Like>());
    }
}
