using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UploadArticleImageServiceTest
{
    // 1x1 ピクセルの PNG 画像
    static readonly byte[] PngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
    );

    readonly IImageStorageGateway imageStorageGateway = Substitute.For<IImageStorageGateway>();
    readonly UploadArticleImageService service;
    readonly User user = new UserBuilder().Build();

    public UploadArticleImageServiceTest()
    {
        this.service = new UploadArticleImageService(this.imageStorageGateway, new TestTimeProvider());
    }

    static IFormFile CreateFile(byte[] bytes, string contentType, long? length = null)
    {
        var file = Substitute.For<IFormFile>();
        file.ContentType.Returns(contentType);
        file.Length.Returns(length ?? bytes.Length);
        file.OpenReadStream().Returns(_ => new MemoryStream(bytes));
        return file;
    }

    async Task AssertRejectedAsync(IFormFile file)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => this.service.ExecuteAsync(this.user.Id, file));

        await this.imageStorageGateway.DidNotReceiveWithAnyArgs().UploadArticleImageAsync("", "", Stream.Null, "");
    }

    [Fact]
    public async Task 画像をアップロードして画像URLを返す()
    {
        this.imageStorageGateway
            .UploadArticleImageAsync(this.user.Id.Value.ToString(), Arg.Any<string>(), Arg.Any<Stream>(), "image/png")
            .Returns("https://images.test/articles/images/1");

        var response = await this.service.ExecuteAsync(this.user.Id, CreateFile(PngBytes, "image/png"));

        Assert.Equal("https://images.test/articles/images/1", response.ImageUrl);
    }

    [Fact]
    public async Task アップロードごとに異なる画像IDを割り当てる()
    {
        var imageIds = new List<string>();
        this.imageStorageGateway
            .UploadArticleImageAsync(Arg.Any<string>(), Arg.Do<string>(imageIds.Add), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://images.test/articles/images/1");

        await this.service.ExecuteAsync(this.user.Id, CreateFile(PngBytes, "image/png"));
        await this.service.ExecuteAsync(this.user.Id, CreateFile(PngBytes, "image/png"));

        Assert.Equal(2, imageIds.Distinct().Count());
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    public async Task 対応していない形式のファイルは拒否する(string contentType)
    {
        await AssertRejectedAsync(CreateFile(PngBytes, contentType));
    }

    [Fact]
    public async Task 上限サイズを超えるファイルは拒否する()
    {
        await AssertRejectedAsync(CreateFile(PngBytes, "image/png", 5_000_001));
    }

    [Fact]
    public async Task 画像として読み込めないファイルは拒否する()
    {
        await AssertRejectedAsync(CreateFile("not an image"u8.ToArray(), "image/png"));
    }
}
