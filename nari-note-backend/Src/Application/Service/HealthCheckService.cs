using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NariNoteBackend.Src.Application.Service
{
    public class HealthCheckService : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("The application is healthy."));
        }
    }
}
