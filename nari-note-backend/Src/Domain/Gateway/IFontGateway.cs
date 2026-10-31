namespace NariNoteBackend.Domain.Gateway;

public interface IFontGateway
{
    /// <summary>text に含まれる文字だけのサブセットフォント（TrueType）を返す。取得できなければ null</summary>
    Task<byte[]?> LoadGoogleFontAsync(string family, int weight, string text);
}
