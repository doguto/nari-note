using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public partial class GoogleFontGateway : IFontGateway
{
    // 取得したサブセットはしばらく使い回す。同じ記事の OG 画像は同じ文字の組み合わせになるため、
    // クローラーの再取得やキャッシュの再検証では Google Fonts に問い合わせずに済む
    // 取得に失敗した場合はキャッシュせず、次のリクエストで取得し直す
    static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(1);

    readonly HttpClient httpClient;
    readonly IMemoryCache cache;
    readonly ILogger<GoogleFontGateway> logger;

    public GoogleFontGateway(HttpClient httpClient, IMemoryCache cache, ILogger<GoogleFontGateway> logger)
    {
        this.httpClient = httpClient;
        this.cache = cache;
        this.logger = logger;
    }

    // User-Agent を付けないと Google Fonts は woff2 ではなく TrueType を返す
    [GeneratedRegex(@"src: url\((.+?)\) format\('(?:opentype|truetype)'\)")]
    private static partial Regex FontUrlPattern();

    public async Task<byte[]?> LoadGoogleFontAsync(string family, int weight, string text)
    {
        var cacheKey = (nameof(GoogleFontGateway), family, weight, text);
        if (this.cache.TryGetValue(cacheKey, out byte[]? cached)) return cached;

        var cssUrl = $"https://fonts.googleapis.com/css2?family={Uri.EscapeDataString(family)}:wght@{weight}&text={Uri.EscapeDataString(text)}";
        try
        {
            var css = await this.httpClient.GetStringAsync(cssUrl);
            var fontUrl = FontUrlPattern().Match(css);
            if (!fontUrl.Success)
            {
                this.logger.LogWarning("Font url not found in css. Family={Family}", family);
                return null;
            }

            var font = await this.httpClient.GetByteArrayAsync(fontUrl.Groups[1].Value);
            return this.cache.Set(cacheKey, font, CacheExpiration);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            this.logger.LogWarning(e, "Failed to load font. Family={Family}", family);
            return null;
        }
    }
}
