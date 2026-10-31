using NariNoteBackend.Application.Asset;

namespace NariNoteBackend.Tests.Application.Asset;

public class KomaImagesTest
{
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
    public void すべての駒の画像を埋め込みリソースから読み込める(string type)
    {
        var image = KomaImages.Get(type);

        Assert.Equal(44, image.Width);
        Assert.Equal(44, image.Height);
    }
}
