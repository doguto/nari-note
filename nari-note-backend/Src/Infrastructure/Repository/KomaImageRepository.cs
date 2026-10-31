using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Infrastructure.Repository;

/// <summary>
///     駒の画像を埋め込みリソース（Asset/Koma/ 配下の PNG）から読み込む。
///     PNG は Scripts/generate_koma_images.py で事前に生成する。
///     シングルトンとして登録し、初回に全種類を読み込んだ後はメモリに保持する
/// </summary>
public class KomaImageRepository : IKomaImageRepository
{
    const string ResourcePrefix = "NariNoteBackend.Infrastructure.Asset.Koma.";

    // 駒の種類 → ファイル名。generate_koma_images.py の PIECES と揃える
    static readonly Dictionary<string, string> FileNames = new()
    {
        ["玉"] = "gyoku", ["王"] = "ou", ["飛"] = "hisha", ["角"] = "kaku", ["金"] = "kin",
        ["銀"] = "gin", ["桂"] = "keima", ["香"] = "kyosha", ["歩"] = "fu",
        ["龍"] = "ryu", ["馬"] = "uma", ["成銀"] = "narigin", ["成桂"] = "narikei", ["成香"] = "narikyo", ["と"] = "tokin"
    };

    readonly Lazy<Dictionary<string, KomaImage>> images = new(
        () => FileNames.ToDictionary(e => e.Key, e => new KomaImage(e.Key, Load(e.Value)))
    );

    public KomaImage FindForceByPieceType(string pieceType)
    {
        return this.images.Value.TryGetValue(pieceType, out var image)
            ? image
            : throw new KeyNotFoundException($"駒の画像がありません: {pieceType}");
    }

    static byte[] Load(string fileName)
    {
        using var stream = typeof(KomaImageRepository).Assembly.GetManifestResourceStream($"{ResourcePrefix}{fileName}.png")
            ?? throw new InvalidOperationException($"埋め込みリソースが見つかりません: {fileName}.png");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
