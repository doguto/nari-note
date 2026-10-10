using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class TagsControllerTest : IntegrationTestBase
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    public TagsControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task 人気タグは直近3か月に公開された記事の件数が多い順に返す()
    {
        var author = new UserBuilder().Build();
        var popular = TestEntity.Tag("振り飛車");
        var normal = TestEntity.Tag("居飛車");
        var old = TestEntity.Tag("古いタグ");
        var draftOnly = TestEntity.Tag("下書きのタグ");
        var recent1 = new ArticleBuilder(author).PublishedAt(Now.AddDays(-1)).Build();
        var recent2 = new ArticleBuilder(author).PublishedAt(Now.AddMonths(-2)).Build();
        var oldArticle = new ArticleBuilder(author).PublishedAt(Now.AddMonths(-3).AddDays(-1)).Build();
        var draft = new ArticleBuilder(author).Draft().Build();
        await SeedAsync(
            author,
            popular,
            normal,
            old,
            draftOnly,
            recent1,
            recent2,
            oldArticle,
            draft,
            TestEntity.ArticleTag(recent1, popular),
            TestEntity.ArticleTag(recent2, popular),
            TestEntity.ArticleTag(oldArticle, popular),
            TestEntity.ArticleTag(recent1, normal),
            TestEntity.ArticleTag(oldArticle, old),
            TestEntity.ArticleTag(draft, draftOnly)
        );

        var response = await CreateClient().GetAsync("/api/tags/popular");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetPopularTagsResponse>(response);
        Assert.Equal(["振り飛車", "居飛車"], body.Tags.Select(t => t.Name));
        // 期間外の記事は件数に含めない
        Assert.Equal([2, 1], body.Tags.Select(t => t.ArticleCount));
    }

    [Fact]
    public async Task 人気タグは上位5件まで返す()
    {
        var author = new UserBuilder().Build();
        var article = new ArticleBuilder(author).Build();
        var tags = Enumerable.Range(1, 6).Select(i => TestEntity.Tag($"タグ{i}")).ToList();
        var entities = new List<object> { author, article };
        entities.AddRange(tags);
        entities.AddRange(tags.Select(tag => (object)TestEntity.ArticleTag(article, tag)));
        await SeedAsync(entities.ToArray());

        var response = await CreateClient().GetAsync("/api/tags/popular");

        var body = await ReadAsync<GetPopularTagsResponse>(response);
        Assert.Equal(5, body.Tags.Count);
    }

    [Fact]
    public async Task 該当するタグが無ければ空の一覧を返す()
    {
        var response = await CreateClient().GetAsync("/api/tags/popular");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await ReadAsync<GetPopularTagsResponse>(response)).Tags);
    }
}
