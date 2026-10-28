using SkiaSharp;

namespace NariNoteBackend.Application.Helper;

public static class ImageFileValidator
{
    public const long MaxFileSizeBytes = 5_000_000;

    static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    // 検証後は Stream を先頭に戻す
    public static void Validate(IFormFile file, Stream stream)
    {
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("対応していない画像形式です。JPEG、PNG、WebPのみ対応しています。");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("ファイルサイズは5MB以内にしてください。");
        }

        if (!IsDecodableImage(stream))
        {
            throw new ArgumentException("ファイルの形式が正しくありません。");
        }

        stream.Seek(0, SeekOrigin.Begin);
    }

    // SKCodec に Stream を直接渡すと codec の破棄時に Stream も破棄されるため、SKData にコピーして渡す。
    // ネイティブライブラリの読み込み失敗を形式エラーとして扱わないよう、例外は捕捉しない。
    static bool IsDecodableImage(Stream stream)
    {
        using var data = SKData.Create(stream);
        if (data == null) return false;

        using var codec = SKCodec.Create(data);
        return codec != null;
    }
}
