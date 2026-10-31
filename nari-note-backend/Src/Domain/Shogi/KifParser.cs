using System.Text.RegularExpressions;

namespace NariNoteBackend.Domain.Shogi;

public record KifMove(int MoveNumber, int ToCol, int ToRow, string Piece, int? FromCol, int? FromRow, bool IsDrop, bool IsPromote);

/// <summary>
///     KIF 形式の棋譜を読み、指定した手数の局面を再現する（フロントエンドの lib/kif-player/utils/parseKif.ts・simulator.ts と同じ解釈をする）
/// </summary>
public static partial class KifParser
{
    static readonly string[] TerminalKeywords = ["投了", "千日手", "中断", "持将棋", "時間切れ", "入玉"];

    static readonly Dictionary<string, string> PromoteMap = new()
    {
        ["飛"] = "龍", ["角"] = "馬", ["銀"] = "成銀", ["桂"] = "成桂", ["香"] = "成香", ["歩"] = "と"
    };

    static readonly Dictionary<string, string> DemoteMap = PromoteMap.ToDictionary(x => x.Value, x => x.Key);

    static readonly string[,] InitialBoard =
    {
        { "v香", "v桂", "v銀", "v金", "v王", "v金", "v銀", "v桂", "v香" },
        { "", "v飛", "", "", "", "", "", "v角", "" },
        { "v歩", "v歩", "v歩", "v歩", "v歩", "v歩", "v歩", "v歩", "v歩" },
        { "", "", "", "", "", "", "", "", "" },
        { "", "", "", "", "", "", "", "", "" },
        { "", "", "", "", "", "", "", "", "" },
        { "歩", "歩", "歩", "歩", "歩", "歩", "歩", "歩", "歩" },
        { "", "角", "", "", "", "", "", "飛", "" },
        { "香", "桂", "銀", "金", "玉", "金", "銀", "桂", "香" }
    };

    // 手数 / 同 / 筋（全角数字） / 段（漢数字） / 駒 / 成 / 打 / 移動元の筋 / 移動元の段
    [GeneratedRegex(@"^\s*([0-9]+)\s+(?:(同[　\s]?)|([１-９])([一二三四五六七八九]))(成[銀桂香]|[玉王飛角金銀桂香歩龍馬と])(成)?(打)?(?:\(([0-9])([0-9])\))?")]
    private static partial Regex MovePattern();

    [GeneratedRegex(@"^\s*([0-9]+)\s+")]
    private static partial Regex MoveNumberPattern();

    public static List<KifMove> ParseMoves(string kifText)
    {
        var moves = new List<KifMove>();
        var lastToCol = 1;
        var lastToRow = 1;

        foreach (var line in kifText.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed == "" || trimmed.StartsWith('*')) continue;

            var numberMatch = MoveNumberPattern().Match(line);
            if (!numberMatch.Success) continue;

            var afterNumber = line[numberMatch.Length..].TrimStart();
            if (TerminalKeywords.Any(afterNumber.StartsWith)) break;

            var m = MovePattern().Match(line);
            if (!m.Success) continue;

            var isSameSquare = m.Groups[2].Success;
            var toCol = isSameSquare ? lastToCol : m.Groups[3].Value[0] - '０';
            var toRow = isSameSquare ? lastToRow : "一二三四五六七八九".IndexOf(m.Groups[4].Value[0]) + 1;

            moves.Add(new KifMove(
                int.Parse(m.Groups[1].Value),
                toCol,
                toRow,
                m.Groups[5].Value,
                m.Groups[8].Success ? int.Parse(m.Groups[8].Value) : null,
                m.Groups[9].Success ? int.Parse(m.Groups[9].Value) : null,
                m.Groups[7].Success,
                m.Groups[6].Success
            ));
            lastToCol = toCol;
            lastToRow = toRow;
        }

        return moves;
    }

    /// <summary>moveNumber 手目を指した後の局面（0 は平手の初期配置）</summary>
    public static ShogiPosition GetPositionAtMove(List<KifMove> moves, int moveNumber)
    {
        var position = CreateInitialPosition();
        foreach (var move in moves.Take(Math.Max(moveNumber, 0)))
        {
            ApplyMove(position, move);
        }

        return position;
    }

    static ShogiPosition CreateInitialPosition()
    {
        var position = new ShogiPosition();
        for (var row = 0; row < ShogiPosition.Size; row++)
        {
            for (var col = 0; col < ShogiPosition.Size; col++)
            {
                var cell = InitialBoard[row, col];
                if (cell == "") continue;

                position.Board[row, col] = cell.StartsWith('v')
                    ? new ShogiPiece(cell[1..], PieceOwner.Gote)
                    : new ShogiPiece(cell, PieceOwner.Sente);
            }
        }

        return position;
    }

    static void ApplyMove(ShogiPosition position, KifMove move)
    {
        var owner = move.MoveNumber % 2 == 1 ? PieceOwner.Sente : PieceOwner.Gote;
        var toRow = move.ToRow - 1;
        var toCol = 9 - move.ToCol;

        if (move.IsDrop)
        {
            RemoveCaptured(position.CapturedOf(owner), move.Piece);
            position.Board[toRow, toCol] = new ShogiPiece(move.Piece, owner);
            return;
        }

        // 移動元が無い指し手は再現できないため飛ばす
        if (move.FromCol is not { } fromCol || move.FromRow is not { } fromRow) return;

        var target = position.Board[toRow, toCol];
        if (target != null && target.Owner != owner)
        {
            AddCaptured(position.CapturedOf(owner), DemoteMap.GetValueOrDefault(target.Type, target.Type));
        }

        var type = move.IsPromote ? PromoteMap.GetValueOrDefault(move.Piece, move.Piece) : move.Piece;
        position.Board[fromRow - 1, 9 - fromCol] = null;
        position.Board[toRow, toCol] = new ShogiPiece(type, owner);
    }

    static void AddCaptured(List<CapturedPiece> captured, string type)
    {
        var existing = captured.Find(p => p.Type == type);
        if (existing != null)
            existing.Count++;
        else
            captured.Add(new CapturedPiece { Type = type, Count = 1 });
    }

    static void RemoveCaptured(List<CapturedPiece> captured, string type)
    {
        var existing = captured.Find(p => p.Type == type);
        if (existing == null) return;

        existing.Count--;
        if (existing.Count <= 0) captured.Remove(existing);
    }
}
