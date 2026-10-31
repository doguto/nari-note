using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using SkiaSharp;

namespace NariNoteBackend.Application.Service;

public class GetArticleOgImageService
{
    const string TextFontFamily = "Noto Sans JP";
    const int FontWeight = 700;

    readonly IArticleRepository articleRepository;
    readonly IFontGateway fontGateway;
    readonly IKomaImageRepository komaImageRepository;
    readonly TimeProvider timeProvider;

    public GetArticleOgImageService(
        IArticleRepository articleRepository,
        IFontGateway fontGateway,
        IKomaImageRepository komaImageRepository,
        TimeProvider timeProvider
    )
    {
        this.articleRepository = articleRepository;
        this.fontGateway = fontGateway;
        this.komaImageRepository = komaImageRepository;
        this.timeProvider = timeProvider;
    }

    /// <summary>盤面・棋譜の埋め込みから局面を描いた PNG を返す。埋め込みが無い記事は存在しないものとして扱う</summary>
    public async Task<byte[]> ExecuteAsync(ArticleId id)
    {
        var article = await this.articleRepository.FindForceByIdAsync(id);
        if (!article.IsPubliclyVisibleAt(this.timeProvider.UtcNow())) throw new KeyNotFoundException($"記事{id}が存在しません");

        var position = ArticleBoardFinder.Find(article.Body, article.Kifus)
            ?? throw new KeyNotFoundException($"記事{id}に盤面の埋め込みがありません");

        var font = await this.fontGateway.LoadGoogleFontAsync(TextFontFamily, FontWeight);
        using var textTypeface = ToTypeface(font);

        var komaImages = position.Board.Cast<ShogiPiece?>()
            .OfType<ShogiPiece>()
            .Select(p => p.Type)
            .Distinct()
            .ToDictionary(type => type, type => SKImage.FromEncodedData(this.komaImageRepository.FindForceByPieceType(type).Png));
        try
        {
            return ArticleOgImageRenderer.Render(article.Title, article.Author.Name, position, textTypeface, komaImages);
        }
        finally
        {
            foreach (var image in komaImages.Values) image.Dispose();
        }
    }

    // フォントが取得できなくても画像は返す（文字は既定のフォントで描く）
    static SKTypeface ToTypeface(byte[]? font)
    {
        if (font == null) return SKTypeface.CreateDefault();

        using var data = SKData.CreateCopy(font);
        return SKTypeface.FromData(data) ?? SKTypeface.CreateDefault();
    }
}
