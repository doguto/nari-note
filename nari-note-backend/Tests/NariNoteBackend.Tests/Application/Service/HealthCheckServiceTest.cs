using Microsoft.Extensions.Diagnostics.HealthChecks;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Tests.Application.Service;

public class HealthCheckServiceTest
{
    [Fact]
    public async Task 正常を示す結果を返す()
    {
        var result = await new NariNoteBackend.Application.Service.HealthCheckService().CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}
