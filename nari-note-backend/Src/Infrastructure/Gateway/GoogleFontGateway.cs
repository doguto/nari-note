using System.Text.RegularExpressions;
using NariNoteBackend.Domain.Gateway;

namespace NariNoteBackend.Infrastructure.Gateway;

public partial class GoogleFontGateway : IFontGateway
{
    readonly HttpClient httpClient;
    readonly ILogger<GoogleFontGateway> logger;

    public GoogleFontGateway(HttpClient httpClient, ILogger<GoogleFontGateway> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    // User-Agent を付けないと Google Fonts は woff2 ではなく TrueType を返す
    [GeneratedRegex(@"src: url\((.+?)\) format\('(?:opentype|truetype)'\)")]
    private static partial Regex FontUrlPattern();

    public async Task<byte[]?> LoadGoogleFontAsync(string family, int weight, string text)
    {
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

            return await this.httpClient.GetByteArrayAsync(fontUrl.Groups[1].Value);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            this.logger.LogWarning(e, "Failed to load font. Family={Family}", family);
            return null;
        }
    }
}
