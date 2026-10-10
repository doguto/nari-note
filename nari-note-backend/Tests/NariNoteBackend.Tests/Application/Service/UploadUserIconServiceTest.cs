using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UploadUserIconServiceTest
{
    // 1x1 ピクセルの PNG 画像
    static readonly byte[] PngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
    );

    readonly IImageStorageGateway imageStorageGateway = Substitute.For<IImageStorageGateway>();
    readonly UploadUserIconService service;
    readonly User user = new UserBuilder().Build();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    public UploadUserIconServiceTest()
    {
        this.userRepository.FindForceByIdAsync(this.user.Id).Returns(this.user);
        this.service = new UploadUserIconService(this.imageStorageGateway, this.userRepository, new TestTimeProvider());
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

        await this.imageStorageGateway.DidNotReceiveWithAnyArgs().UploadUserIconAsync("", Stream.Null, "");
        await this.userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact(Skip = "既知の不具合 (#574): 形式検証で SKCodec がストリームを破棄し後続の Seek が失敗する。Linux では libSkiaSharp も読み込めない")]
    public async Task 画像をアップロードしてプロフィール画像を更新する()
    {
        this.imageStorageGateway
            .UploadUserIconAsync(this.user.Id.Value.ToString(), Arg.Any<Stream>(), "image/png")
            .Returns("https://images.test/icon");

        var response = await this.service.ExecuteAsync(this.user.Id, CreateFile(PngBytes, "image/png"));

        Assert.Equal("https://images.test/icon", response.UserIconImageUrl);
        Assert.Equal("https://images.test/icon", this.user.ProfileImage);
        Assert.Equal(TestTimeProvider.DefaultUtcNow, this.user.UpdatedAt);
        await this.userRepository.Received(1).UpdateAsync(this.user);
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
