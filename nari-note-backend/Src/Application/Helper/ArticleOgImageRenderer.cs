using System.Globalization;
using NariNoteBackend.Domain.Shogi;
using SkiaSharp;

namespace NariNoteBackend.Application.Helper;

/// <summary>
///     記事の OG 画像（左に局面、右にタイトル）を描く。
///     見た目はフロントエンドの盤面（globals.css の --color-shogi-*、lib/kif-player/constants の KOMA_SHAPE_POINTS）に揃える
/// </summary>
public static class ArticleOgImageRenderer
{
    public const int Width = 1200;
    public const int Height = 630;
    public const string BrandName = "なりノート";

    const float Padding = 40;
    const float CellSize = 50;
    const float BoardBorder = 3;
    const float BoardSize = CellSize * ShogiPosition.Size + BoardBorder * 2;
    const float PieceSize = 44;
    const float CapturedRowHeight = 36;
    const float CapturedRowGap = 8;
    const float TitleFontSize = 54;
    const float TitleLineHeight = TitleFontSize * 1.35f;
    const int TitleMaxLines = 5;
    const float AuthorFontSize = 32;
    const float BrandFontSize = 36;
    const float FooterLineHeightRatio = 1.45f;

    static readonly SKColor Background = SKColor.Parse("#f5f3e8");
    static readonly SKColor BoardColor = SKColor.Parse("#e9c98a");
    static readonly SKColor BoardLine = SKColor.Parse("#5a4320");
    static readonly SKColor Koma = SKColor.Parse("#f6e2b3");
    static readonly SKColor KomaEdge = SKColor.Parse("#8a6a3a");
    static readonly SKColor KomaInk = SKColor.Parse("#2a1d0c");
    static readonly SKColor KomaPromoted = SKColor.Parse("#b3261e");
    static readonly SKColor TitleColor = SKColor.Parse("#1a2515");
    static readonly SKColor TextColor = SKColor.Parse("#2d3e1f");
    static readonly SKColor BrandColor = SKColor.Parse("#88b04b");

    // viewBox 0 0 60 60 の五角形（先手向き）
    static readonly SKPoint[] KomaShape = [new(30, 5), new(49, 12), new(54, 56), new(6, 56), new(11, 12)];

    static readonly Dictionary<string, string> KomaLabels = new()
    {
        ["成銀"] = "全", ["成桂"] = "圭", ["成香"] = "杏"
    };

    public static string KomaLabel(string type)
    {
        return KomaLabels.GetValueOrDefault(type, type);
    }

    public static string FormatCaptured(List<CapturedPiece> pieces)
    {
        if (pieces.Count == 0) return "なし";
        return string.Join(" ", pieces.Select(p => p.Count > 1 ? $"{p.Type}{p.Count}" : p.Type));
    }

    public static byte[] Render(string title, string authorName, ShogiPosition position, SKTypeface textTypeface, SKTypeface komaTypeface)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var canvas = surface.Canvas;
        canvas.Clear(Background);

        var columnHeight = CapturedRowHeight * 2 + CapturedRowGap * 2 + BoardSize;
        var top = (Height - columnHeight) / 2;
        var boardTop = top + CapturedRowHeight + CapturedRowGap;
        var columnCenterX = Padding + BoardSize / 2;

        using var capturedFont = new SKFont(textTypeface, 26);
        DrawTextCentered(canvas, $"△ 持駒 {FormatCaptured(position.GoteCaptured)}", columnCenterX, top + CapturedRowHeight / 2, capturedFont, TextColor);
        DrawBoard(canvas, position, Padding, boardTop, komaTypeface);
        DrawTextCentered(canvas, $"▲ 持駒 {FormatCaptured(position.SenteCaptured)}", columnCenterX, boardTop + BoardSize + CapturedRowGap + CapturedRowHeight / 2, capturedFont, TextColor);

        var textLeft = Padding + BoardSize + 56;
        var textWidth = Width - Padding - textLeft;
        DrawTitle(canvas, title, textLeft, Padding + 16, textWidth, textTypeface);
        DrawFooter(canvas, authorName, textLeft, Height - Padding - 16, textTypeface);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    static void DrawBoard(SKCanvas canvas, ShogiPosition position, float left, float top, SKTypeface komaTypeface)
    {
        using var boardPaint = new SKPaint { Color = BoardColor, IsAntialias = true };
        canvas.DrawRect(left, top, BoardSize, BoardSize, boardPaint);

        using var borderPaint = new SKPaint { Color = BoardLine, Style = SKPaintStyle.Stroke, StrokeWidth = BoardBorder, IsAntialias = true };
        canvas.DrawRect(left + BoardBorder / 2, top + BoardBorder / 2, BoardSize - BoardBorder, BoardSize - BoardBorder, borderPaint);

        var innerLeft = left + BoardBorder;
        var innerTop = top + BoardBorder;
        var innerSize = CellSize * ShogiPosition.Size;
        using var linePaint = new SKPaint { Color = BoardLine, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        for (var i = 1; i < ShogiPosition.Size; i++)
        {
            var offset = CellSize * i;
            canvas.DrawLine(innerLeft + offset, innerTop, innerLeft + offset, innerTop + innerSize, linePaint);
            canvas.DrawLine(innerLeft, innerTop + offset, innerLeft + innerSize, innerTop + offset, linePaint);
        }

        using var starPaint = new SKPaint { Color = BoardLine, IsAntialias = true };
        foreach (var r in new[] { 3, 6 })
        {
            foreach (var c in new[] { 3, 6 })
            {
                canvas.DrawCircle(innerLeft + CellSize * c, innerTop + CellSize * r, 4, starPaint);
            }
        }

        using var komaFont = new SKFont(komaTypeface, 22);
        for (var row = 0; row < ShogiPosition.Size; row++)
        {
            for (var col = 0; col < ShogiPosition.Size; col++)
            {
                var piece = position.Board[row, col];
                if (piece == null) continue;

                DrawKoma(canvas, piece, innerLeft + CellSize * (col + 0.5f), innerTop + CellSize * (row + 0.5f), komaFont);
            }
        }
    }

    static void DrawKoma(SKCanvas canvas, ShogiPiece piece, float centerX, float centerY, SKFont font)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);
        if (piece.Owner == PieceOwner.Gote) canvas.RotateDegrees(180);

        var scale = PieceSize / 60;
        using var path = new SKPath();
        path.AddPoly(KomaShape.Select(p => new SKPoint(p.X * scale - PieceSize / 2, p.Y * scale - PieceSize / 2)).ToArray());

        using var fillPaint = new SKPaint { Color = Koma, IsAntialias = true };
        canvas.DrawPath(path, fillPaint);
        using var edgePaint = new SKPaint
        {
            Color = KomaEdge, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f * scale, StrokeJoin = SKStrokeJoin.Round, IsAntialias = true
        };
        canvas.DrawPath(path, edgePaint);

        DrawTextCentered(canvas, KomaLabel(piece.Type), 0, 3, font, piece.IsPromoted ? KomaPromoted : KomaInk);
        canvas.Restore();
    }

    static void DrawTitle(SKCanvas canvas, string title, float left, float top, float maxWidth, SKTypeface typeface)
    {
        using var font = new SKFont(typeface, TitleFontSize);
        var lines = WrapText(title, font, maxWidth, TitleMaxLines);
        for (var i = 0; i < lines.Count; i++)
        {
            DrawTextLeft(canvas, lines[i], left, top + TitleLineHeight * (i + 0.5f), font, TitleColor);
        }
    }

    static void DrawFooter(SKCanvas canvas, string authorName, float left, float bottom, SKTypeface typeface)
    {
        var brandHeight = BrandFontSize * FooterLineHeightRatio;
        var authorHeight = AuthorFontSize * FooterLineHeightRatio;

        using var brandFont = new SKFont(typeface, BrandFontSize);
        DrawTextLeft(canvas, BrandName, left, bottom - brandHeight / 2, brandFont, BrandColor);

        using var authorFont = new SKFont(typeface, AuthorFontSize);
        DrawTextLeft(canvas, authorName, left, bottom - brandHeight - 16 - authorHeight / 2, authorFont, TextColor);
    }

    /// <summary>文字単位で折り返し、maxLines を超える分は末尾を … にする</summary>
    static List<string> WrapText(string text, SKFont font, float maxWidth, int maxLines)
    {
        var lines = new List<string>();
        var current = "";
        foreach (var element in TextElements(text))
        {
            if (current != "" && font.MeasureText(current + element) > maxWidth)
            {
                lines.Add(current);
                current = "";
            }

            current += element;
        }

        if (current != "") lines.Add(current);
        if (lines.Count <= maxLines) return lines;

        var last = lines[maxLines - 1];
        while (last != "" && font.MeasureText(last + "…") > maxWidth)
        {
            last = last[..^1];
        }

        return [.. lines.Take(maxLines - 1), last + "…"];
    }

    static IEnumerable<string> TextElements(string text)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            yield return enumerator.GetTextElement();
        }
    }

    static void DrawTextCentered(SKCanvas canvas, string text, float centerX, float centerY, SKFont font, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawText(text, centerX, Baseline(centerY, font), SKTextAlign.Center, font, paint);
    }

    static void DrawTextLeft(SKCanvas canvas, string text, float left, float centerY, SKFont font, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawText(text, left, Baseline(centerY, font), SKTextAlign.Left, font, paint);
    }

    static float Baseline(float centerY, SKFont font)
    {
        var metrics = font.Metrics;
        return centerY - (metrics.Ascent + metrics.Descent) / 2;
    }
}
