using SkiaSharp;

namespace NariNoteBackend.Application.Asset;

/// <summary>
///     OG 画像の盤面に置く駒の画像（先手向き）
///     Koma/ 配下の PNG（埋め込みリソース）は scripts/generate_koma_assets.py（リポジトリ直下）で、
///     フロントエンドの駒（KomaImage）と同じ SVG から事前に生成する
/// </summary>
public static class KomaImages
{
    const string ResourcePrefix = "NariNoteBackend.Application.Asset.Koma.";

    // 駒の種類 → ファイル名。generate_koma_assets.py の PNG_FILE_NAMES と揃える
    static readonly Dictionary<string, string> FileNames = new()
    {
        ["玉"] = "gyoku", ["王"] = "ou", ["飛"] = "hisha", ["角"] = "kaku", ["金"] = "kin",
        ["銀"] = "gin", ["桂"] = "keima", ["香"] = "kyosha", ["歩"] = "fu",
        ["龍"] = "ryu", ["馬"] = "uma", ["成銀"] = "narigin", ["成桂"] = "narikei", ["成香"] = "narikyo", ["と"] = "tokin"
    };

    static readonly Dictionary<string, SKImage> Images = FileNames.ToDictionary(e => e.Key, e => Load(e.Value));

    public static SKImage Get(string type)
    {
        return Images.TryGetValue(type, out var image) ? image : throw new ArgumentException($"駒の画像がありません: {type}", nameof(type));
    }

    static SKImage Load(string fileName)
    {
        using var stream = typeof(KomaImages).Assembly.GetManifestResourceStream($"{ResourcePrefix}{fileName}.png")
            ?? throw new InvalidOperationException($"埋め込みリソースが見つかりません: {fileName}.png");
        using var data = SKData.Create(stream);
        return SKImage.FromEncodedData(data);
    }
}
