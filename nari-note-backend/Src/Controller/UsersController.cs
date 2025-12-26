using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Application.Exception;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly GetUserProfileService getUserProfileService;
    
    public UsersController(GetUserProfileService getUserProfileService)
    {
        this.getUserProfileService = getUserProfileService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfile(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "ユーザーIDは1以上の値を指定してください" });
        }
        
        try
        {
            var request = new GetUserProfileRequest { Id = id };
            var response = await this.getUserProfileService.ExecuteAsync(request);
            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound(new GetUserProfileNotFoundResponse());
        }
    }
}
