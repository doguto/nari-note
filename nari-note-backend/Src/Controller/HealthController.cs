using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    GetHealthService getHealthService;

    public HealthController(GetHealthService getHealthService)
    {
        this.getHealthService = getHealthService;
    }

    [HttpGet]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult> GetHealth()
    {
        var response = await getHealthService.ExecuteAsync();
        return Ok(response);
    }
}
