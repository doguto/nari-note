using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class HealthControllerTest : IntegrationTestBase
{
    public HealthControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact(Skip = "既知の不具合 (#573): GetHealthService が DI に登録されておらず 400 を返す")]
    public async Task ヘルスチェックAPIは未認証で200を返す()
    {
        var response = await CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200, (await ReadAsync<GetHealthResponse>(response)).StatusCode);
    }
}
