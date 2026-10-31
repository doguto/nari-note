using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NariNoteBackend.Infrastructure.Gateway;

namespace NariNoteBackend.Tests.Infrastructure.Gateway;

public class GoogleFontGatewayTest
{
    const string FontUrl = "https://fonts.gstatic.com/l/font?kit=test";
    static readonly byte[] Font = [1, 2, 3];

    readonly StubHandler handler = new();
    readonly GoogleFontGateway gateway;

    public GoogleFontGatewayTest()
    {
        this.gateway = new GoogleFontGateway(
            new HttpClient(this.handler),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GoogleFontGateway>.Instance
        );
    }

    [Fact]
    public async Task 同じ文字の組み合わせは取得済みのフォントを返しGoogleFontsに問い合わせない()
    {
        var first = await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "なりノート");
        var second = await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "なりノート");

        Assert.Equal(Font, first);
        Assert.Equal(Font, second);
        Assert.Equal(2, this.handler.RequestCount);
    }

    [Fact]
    public async Task 文字の組み合わせが異なれば取得し直す()
    {
        await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "なりノート");
        await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "将棋");

        Assert.Equal(4, this.handler.RequestCount);
    }

    [Fact]
    public async Task 取得に失敗した結果はキャッシュせず次のリクエストで取得し直す()
    {
        this.handler.Fail = true;
        Assert.Null(await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "なりノート"));

        this.handler.Fail = false;
        Assert.Equal(Font, await this.gateway.LoadGoogleFontAsync("Noto Sans JP", 700, "なりノート"));
    }

    class StubHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public bool Fail { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.RequestCount++;
            if (this.Fail) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            var content = request.RequestUri!.Host == "fonts.googleapis.com"
                ? new StringContent($"@font-face {{ src: url({FontUrl}) format('truetype'); }}")
                : (HttpContent)new ByteArrayContent(Font);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
