using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Tests.Application.Service;

public class GetHealthServiceTest
{
    [Fact]
    public async Task 正常を示すステータスを返す()
    {
        var response = await new GetHealthService().ExecuteAsync();

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("", response.Message);
    }
}
