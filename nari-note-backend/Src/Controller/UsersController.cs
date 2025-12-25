using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly GetUserProfileService _getUserProfileService;
    
    public UsersController(GetUserProfileService getUserProfileService)
    {
        _getUserProfileService = getUserProfileService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserProfileResponse>> GetUserProfile(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "ユーザーIDは1以上の値を指定してください" });
        }
        
        var request = new GetUserProfileRequest { Id = id };
        var response = await _getUserProfileService.ExecuteAsync(request);
        
        if (response == null)
        {
            return NotFound(new GetUserProfileNotFoundResponse { UserId = id });
        }
        
        return Ok(response);
    }
}
