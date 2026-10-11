using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NariNoteBackend.Tests.Application.Service;

public class CreateCommentServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly ICommentRepository commentRepository = Substitute.For<ICommentRepository>();
    readonly CreateCommentService service;
    readonly User user = new UserBuilder().Build();

    public CreateCommentServiceTest()
    {
        this.service = new CreateCommentService(this.commentRepository, this.articleRepository);
    }

    [Fact]
    public async Task 記事にコメントを作成する()
    {
        var article = new ArticleBuilder(new UserBuilder().Build()).Build();
        this.articleRepository.FindForceByIdAsync(article.Id).Returns(article);
        this.commentRepository
            .CreateAsync(Arg.Any<Comment>())
            .Returns(call =>
            {
                var comment = call.Arg<Comment>();
                comment.Id = CommentId.From(7);
                comment.CreatedAt = TestTimeProvider.DefaultUtcNow;
                return comment;
            });

        var response = await this.service.ExecuteAsync(
            this.user.Id,
            new CreateCommentRequest { ArticleId = article.Id, Message = "勉強になりました" }
        );

        Assert.Equal(CommentId.From(7), response.Id);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, response.CreatedAt);
        await this.commentRepository.Received(1).CreateAsync(
            Arg.Is<Comment>(c =>
                c.UserId == this.user.Id && c.ArticleId == article.Id && c.Message == "勉強になりました"
            )
        );
    }

    [Fact]
    public async Task 存在しない記事にはコメントを作成しない()
    {
        var articleId = ArticleId.From(Guid.CreateVersion7());
        this.articleRepository.FindForceByIdAsync(articleId).ThrowsAsync(new KeyNotFoundException());
        var request = new CreateCommentRequest { ArticleId = articleId, Message = "コメント" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(this.user.Id, request));

        await this.commentRepository.DidNotReceive().CreateAsync(Arg.Any<Comment>());
    }
}
