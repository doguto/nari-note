using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Fake;
using NSubstitute;
using SkiaSharp;

namespace NariNoteBackend.Tests.Application.Service;

public class GetArticleOgImageServiceTest
{
    const string BodBody = """
                           ```bod
                           後手の持駒：なし
                           +---------------------------+
                           | ・ ・ ・ ・v玉 ・ ・ ・ ・|一
                           +---------------------------+
                           先手の持駒：金
                           ```
                           """;

    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly User author = new UserBuilder().Build();
    readonly FakeFontGateway fontGateway = new();
    readonly GetArticleOgImageService service;
    readonly TestTimeProvider timeProvider = new();

    public GetArticleOgImageServiceTest()
    {
        this.service = new GetArticleOgImageService(this.articleRepository, this.fontGateway, this.timeProvider);
    }

    Article SetupArticle(ArticleBuilder builder)
    {
        var article = builder.Build();
        this.articleRepository.FindForceByIdAsync(article.Id).Returns(article);
        return article;
    }

    [Fact]
    public async Task 盤面埋め込みのある記事はOG画像のPNGを返す()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).WithBody(BodBody));

        var png = await this.service.ExecuteAsync(article.Id);

        using var codec = SKCodec.Create(new MemoryStream(png));
        Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
        Assert.Equal(new SKSizeI(1200, 630), codec.Info.Size);
        Assert.Equal(["Noto Sans JP", "Shippori Mincho"], this.fontGateway.RequestedFamilies);
    }

    [Fact]
    public async Task 盤面と棋譜の埋め込みが無い記事は存在しないものとして扱う()
    {
        var article = SetupArticle(new ArticleBuilder(this.author).WithBody("本文だけ"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(article.Id));
    }

    [Fact]
    public async Task 未公開の記事は存在しないものとして扱う()
    {
        var draft = SetupArticle(new ArticleBuilder(this.author).WithBody(BodBody).Draft());
        var scheduled = SetupArticle(
            new ArticleBuilder(this.author).WithBody(BodBody).PublishedAt(TestTimeProvider.DefaultUtcNow.AddHours(1))
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(draft.Id));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(scheduled.Id));
    }
}
