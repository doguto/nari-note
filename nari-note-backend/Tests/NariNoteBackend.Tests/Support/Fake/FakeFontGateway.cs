using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Tests.Support.Fake;

/// <summary>Google Fonts へは接続せず、フォントを取得できなかったものとして扱う</summary>
public class FakeFontGateway : IFontGateway
{
    public List<string> RequestedFamilies { get; } = new();

    public Task<byte[]?> LoadGoogleFontAsync(string family, int weight, string text)
    {
        this.RequestedFamilies.Add(family);
        return Task.FromResult<byte[]?>(null);
    }
}
