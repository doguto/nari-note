using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain;
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
    const string FixedText = "△▲持駒なし0123456789 …";

    readonly IArticleRepository articleRepository;
    readonly IFontGateway fontGateway;
    readonly TimeProvider timeProvider;

    public GetArticleOgImageService(IArticleRepository articleRepository, IFontGateway fontGateway, TimeProvider timeProvider)
    {
        this.articleRepository = articleRepository;
        this.fontGateway = fontGateway;
        this.timeProvider = timeProvider;
    }

    /// <summary>盤面・棋譜の埋め込みから局面を描いた PNG を返す。埋め込みが無い記事は存在しないものとして扱う</summary>
    public async Task<byte[]> ExecuteAsync(ArticleId id)
    {
        var article = await this.articleRepository.FindForceByIdAsync(id);
        if (!article.IsPubliclyVisibleAt(this.timeProvider.UtcNow())) throw new KeyNotFoundException($"記事{id}が存在しません");

        var position = ArticleBoardFinder.Find(article.Body, article.Kifus)
            ?? throw new KeyNotFoundException($"記事{id}に盤面の埋め込みがありません");

        var text = string.Concat(
            article.Title,
            article.Author.Name,
            Brand.Name,
            FixedText,
            ArticleOgImageRenderer.FormatCaptured(position.SenteCaptured),
            ArticleOgImageRenderer.FormatCaptured(position.GoteCaptured)
        );
        // 駒は事前に生成した画像を貼るため、取得するのはタイトル等の文字のフォントだけ
        var font = await this.fontGateway.LoadGoogleFontAsync(TextFontFamily, FontWeight, text);

        using var textTypeface = ToTypeface(font);
        return ArticleOgImageRenderer.Render(article.Title, article.Author.Name, position, textTypeface);
    }

    // フォントが取得できなくても画像は返す（文字は既定のフォントで描く）
    static SKTypeface ToTypeface(byte[]? font)
    {
        if (font == null) return SKTypeface.CreateDefault();

        using var data = SKData.CreateCopy(font);
        return SKTypeface.FromData(data) ?? SKTypeface.CreateDefault();
    }
}
