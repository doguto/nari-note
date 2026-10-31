using NariNoteBackend.Infrastructure.Repository;
using SkiaSharp;

namespace NariNoteBackend.Tests.Infrastructure.Repository;

public class KomaImageRepositoryTest
{
    readonly KomaImageRepository repository = new();

    [Theory]
    [InlineData("玉")]
    [InlineData("王")]
    [InlineData("飛")]
    [InlineData("角")]
    [InlineData("金")]
    [InlineData("銀")]
    [InlineData("桂")]
    [InlineData("香")]
    [InlineData("歩")]
    [InlineData("龍")]
    [InlineData("馬")]
    [InlineData("成銀")]
    [InlineData("成桂")]
    [InlineData("成香")]
    [InlineData("と")]
    public void すべての駒の画像を埋め込みリソースから読み込める(string pieceType)
    {
        var image = this.repository.FindForceByPieceType(pieceType);

        using var codec = SKCodec.Create(new MemoryStream(image.Png));
        Assert.Equal(pieceType, image.PieceType);
        Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
        Assert.Equal(new SKSizeI(44, 44), codec.Info.Size);
    }

    [Fact]
    public void 存在しない駒の種類は例外を投げる()
    {
        Assert.Throws<KeyNotFoundException>(() => this.repository.FindForceByPieceType("象"));
    }
}
