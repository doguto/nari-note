using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NariNoteBackend.Src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase, IHealthCheck
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "Healthy" });
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("The application is healthy."));
        }
    }
}
