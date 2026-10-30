using System.Text.RegularExpressions;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Shogi;

/// <summary>
///     記事の OG 画像に描く局面を本文の埋め込みから決める（フロントエンドの lib/og/findArticleBoard.ts と同じ優先順位）。
///     盤面埋め込みを棋譜埋め込みより優先し、それぞれ解析できる最初のものを使う
/// </summary>
public static partial class ArticleBoardFinder
{
    [GeneratedRegex(@"^```(kifu|bod)[ \t]*\n([\s\S]*?)\n```[ \t]*$", RegexOptions.Multiline)]
    private static partial Regex EmbedBlockPattern();

    public static ShogiPosition? Find(string body, IEnumerable<Kifu> kifus)
    {
        var blocks = EmbedBlockPattern()
                     .Matches(body.Replace("\r\n", "\n"))
                     .Select(m => (Lang: m.Groups[1].Value, Content: m.Groups[2].Value))
                     .ToList();

        foreach (var block in blocks.Where(b => b.Lang == "bod"))
        {
            var position = BodParser.Parse(block.Content);
            if (position.HasAnyPiece) return position;
        }

        foreach (var block in blocks.Where(b => b.Lang == "kifu"))
        {
            var (name, moveNumber) = ParseKifuEmbed(block.Content);
            var kifu = kifus.FirstOrDefault(k => k.Name == name);
            if (kifu == null) continue;

            return KifParser.GetPositionAtMove(KifParser.ParseMoves(kifu.KifuText), moveNumber);
        }

        return null;
    }

    /// <summary>`棋譜名_手数` を分解する。末尾が数値でなければ全体を棋譜名、手数 0 とする</summary>
    static (string Name, int MoveNumber) ParseKifuEmbed(string text)
    {
        var raw = text.Trim();
        var lastUnderscore = raw.LastIndexOf('_');
        if (lastUnderscore != -1 && int.TryParse(raw[(lastUnderscore + 1)..], out var moveNumber))
        {
            return (raw[..lastUnderscore], moveNumber);
        }

        return (raw, 0);
    }
}
