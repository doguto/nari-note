using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class CorsPolicyTest : IntegrationTestBase
{
    public CorsPolicyTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task 本番のoriginからのリクエストはCORSで許可される()
    {
        var response = await SendWithOriginAsync("https://nari-note.com");

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Equal(["https://nari-note.com"], origins);
    }

    [Fact]
    public async Task 開発用のoriginからのリクエストは開発環境以外ではCORSで許可されない()
    {
        var response = await SendWithOriginAsync("http://localhost:3000");

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    async Task<HttpResponseMessage> SendWithOriginAsync(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        request.Headers.Add("Origin", origin);
        return await CreateClient().SendAsync(request);
    }
}
