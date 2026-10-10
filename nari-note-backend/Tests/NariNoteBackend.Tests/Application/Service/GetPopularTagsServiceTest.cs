using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetPopularTagsServiceTest
{
    readonly GetPopularTagsService service;
    readonly ITagRepository tagRepository = Substitute.For<ITagRepository>();

    public GetPopularTagsServiceTest()
    {
        this.service = new GetPopularTagsService(this.tagRepository, new TestTimeProvider());
    }

    static Tag CreateTag(string name, int articleCount)
    {
        var tag = TestEntity.Tag(name);
        tag.ArticleTags.AddRange(Enumerable.Range(0, articleCount).Select(_ => new ArticleTag()));
        return tag;
    }

    [Fact]
    public async Task 直近3か月の上位5件のタグを記事数と合わせて返す()
    {
        var since = TestTimeProvider.DefaultUtcNow.AddMonths(-3);
        this.tagRepository
            .GetPopularTagsAsync(since, 5)
            .Returns(new List<Tag> { CreateTag("振り飛車", 3), CreateTag("居飛車", 1) });

        var response = await this.service.ExecuteAsync(new GetPopularTagsRequest());

        Assert.Equal(["振り飛車", "居飛車"], response.Tags.Select(t => t.Name));
        Assert.Equal([3, 1], response.Tags.Select(t => t.ArticleCount));
    }
}
