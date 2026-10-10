using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class DeleteArticleServiceTest
{
    readonly Article article;
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().Build();
    readonly DeleteArticleService service;

    public DeleteArticleServiceTest()
    {
        this.article = new ArticleBuilder(this.author).Build();
        this.articleRepository.FindForceByIdAsync(this.article.Id).Returns(this.article);
        this.service = new DeleteArticleService(this.articleRepository);
    }

    [Fact]
    public async Task 作者は記事を削除できる()
    {
        await this.service.ExecuteAsync(this.author.Id, new DeleteArticleRequest { Id = this.article.Id });

        await this.articleRepository.Received(1).DeleteAsync(this.article.Id);
    }

    [Fact]
    public async Task 作者以外は記事を削除できない()
    {
        var otherUser = new UserBuilder().Build();
        var request = new DeleteArticleRequest { Id = this.article.Id };

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(otherUser.Id, request));

        await this.articleRepository.DidNotReceiveWithAnyArgs().DeleteAsync(this.article.Id);
    }
}
