using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    readonly GetUserProfileService getUserProfileService;
    
    public UsersController(GetUserProfileService getUserProfileService)
    {
        this.getUserProfileService = getUserProfileService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfile(int id)
    {
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var request = new GetUserProfileRequest { Id = id };
        var response = await getUserProfileService.ExecuteAsync(request);
        return Ok(response);
    }
}
