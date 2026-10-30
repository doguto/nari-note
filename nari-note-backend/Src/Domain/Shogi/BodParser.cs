using System.Text.RegularExpressions;

namespace NariNoteBackend.Domain.Shogi;

/// <summary>
///     BOD 形式の盤面を読む（フロントエンドの lib/kif-player/utils/parseBOD.ts と同じ解釈をする）
/// </summary>
public static partial class BodParser
{
    const string EmptySquare = "・";

    static readonly Dictionary<char, int> KanjiNumbers = new()
    {
        ['〇'] = 0, ['零'] = 0, ['一'] = 1, ['二'] = 2, ['三'] = 3, ['四'] = 4, ['五'] = 5,
        ['六'] = 6, ['七'] = 7, ['八'] = 8, ['九'] = 9, ['十'] = 10
    };

    static readonly HashSet<string> PieceTypes = ["玉", "王", "飛", "角", "金", "銀", "桂", "香", "歩", "龍", "馬", "成銀", "成桂", "成香", "と"];

    [GeneratedRegex(@"\|(.+)\|")]
    private static partial Regex BoardRowPattern();

    [GeneratedRegex(@"^(v?[玉王飛角金銀桂香歩龍馬と]|成[銀桂香])(.*)$")]
    private static partial Regex CapturedPiecePattern();

    [GeneratedRegex(@"[\s　]+")]
    private static partial Regex SpacePattern();

    public static ShogiPosition Parse(string bodText)
    {
        var lines = bodText.Split('\n').Select(l => l.Trim()).ToArray();
        var position = new ShogiPosition();
        var boardStartIndex = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.StartsWith("後手の持駒："))
            {
                position.GoteCaptured.AddRange(ParseCapturedPieces(line["後手の持駒：".Length..].Trim()));
            }
            else if (line.StartsWith("先手の持駒："))
            {
                position.SenteCaptured.AddRange(ParseCapturedPieces(line["先手の持駒：".Length..].Trim()));
            }
            else if (line.StartsWith('+') && line.Contains('-') && boardStartIndex == -1)
            {
                boardStartIndex = i + 1;
            }
            else if (boardStartIndex != -1 && i >= boardStartIndex && i < boardStartIndex + ShogiPosition.Size)
            {
                ParseBoardRow(line, i - boardStartIndex, position);
            }
        }

        return position;
    }

    static IEnumerable<CapturedPiece> ParseCapturedPieces(string text)
    {
        if (text is "" or "なし") yield break;

        foreach (var part in SpacePattern().Split(text).Where(p => p != ""))
        {
            var match = CapturedPiecePattern().Match(part);
            if (!match.Success) continue;

            var count = match.Groups[2].Value == "" ? 1 : ParseJapaneseNumber(match.Groups[2].Value);
            yield return new CapturedPiece { Type = match.Groups[1].Value.TrimStart('v'), Count = count };
        }
    }

    static int ParseJapaneseNumber(string text)
    {
        var result = 0;
        var hasJuu = false;
        var juuMultiplier = 0;

        foreach (var c in text)
        {
            if (c == '十')
            {
                hasJuu = true;
                result += juuMultiplier > 0 ? juuMultiplier * 10 : 10;
                juuMultiplier = 0;
            }
            else if (KanjiNumbers.TryGetValue(c, out var num))
            {
                if (hasJuu)
                {
                    result += num;
                    hasJuu = false;
                }
                else
                {
                    juuMultiplier = num;
                }
            }
        }

        if (juuMultiplier > 0 && !hasJuu) result += juuMultiplier;
        return result == 0 ? 1 : result;
    }

    static void ParseBoardRow(string line, int row, ShogiPosition position)
    {
        var match = BoardRowPattern().Match(line);
        if (!match.Success) return;

        var content = match.Groups[1].Value;
        var col = 0;

        for (var i = 0; i < content.Length && col < ShogiPosition.Size;)
        {
            if (content[i] is ' ' or '　')
            {
                i++;
                continue;
            }

            var isGote = content[i] == 'v' && i + 1 < content.Length;
            if (isGote) i++;

            var pieceStr = content[i].ToString();
            i++;

            if (pieceStr == EmptySquare)
            {
                position.Board[row, col++] = null;
                continue;
            }

            if (pieceStr == "成" && i < content.Length && content[i] is '銀' or '桂' or '香')
            {
                pieceStr += content[i];
                i++;
            }

            position.Board[row, col++] = PieceTypes.Contains(pieceStr)
                ? new ShogiPiece(pieceStr, isGote ? PieceOwner.Gote : PieceOwner.Sente)
                : null;
        }
    }
}
