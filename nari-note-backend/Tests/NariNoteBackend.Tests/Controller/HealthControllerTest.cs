using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class HealthControllerTest : IntegrationTestBase
{
    public HealthControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ヘルスチェックAPIは未認証で200を返す()
    {
        var response = await CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200, (await ReadAsync<GetHealthResponse>(response)).StatusCode);
    }

    [Fact]
    public async Task ヘルスチェックエンドポイントは未認証で200を返す()
    {
        var response = await CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
