using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetDraftArticlesServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly GetDraftArticlesService service;

    public GetDraftArticlesServiceTest()
    {
        this.service = new GetDraftArticlesService(this.articleRepository);
    }

    [Fact]
    public async Task 自分の下書きを返す()
    {
        var author = new UserBuilder().WithName("author").Build();
        var draft = new ArticleBuilder(author).WithTitle("下書き").WithBody("書きかけ").WithTags("振り飛車").Draft().Build();
        this.articleRepository.FindDraftsByAuthorAsync(author.Id).Returns(new List<Article> { draft });

        var response = await this.service.ExecuteAsync(author.Id);

        var dto = Assert.Single(response.Articles);
        Assert.Equal(draft.Id, dto.Id);
        Assert.Equal("下書き", dto.Title);
        Assert.Equal("書きかけ", dto.Body);
        Assert.Equal("author", dto.AuthorName);
        Assert.Equal(["振り飛車"], dto.Tags);
        Assert.False(dto.IsPublished);
        Assert.Null(dto.PublishedAt);
        Assert.Equal(0, dto.LikeCount);
    }
}
