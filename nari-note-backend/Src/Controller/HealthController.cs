using Microsoft.AspNetCore.Mvc;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "Healthy" });
    }
}
