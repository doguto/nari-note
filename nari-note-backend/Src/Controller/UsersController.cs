using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
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
        if (id <= 0)
        {
            return BadRequest(new { Message = "ユーザーIDは1以上の値を指定してください" });
        }
        
        var request = new GetUserProfileRequest { Id = id };
        var response = await this.getUserProfileService.ExecuteAsync(request);
        
        return response switch
        {
            GetUserProfileResponse profileResponse => Ok(profileResponse),
            GetUserProfileNotFoundResponse notFoundResponse => NotFound(notFoundResponse),
            GetUserProfileBadRequestResponse badRequestResponse => BadRequest(badRequestResponse),
            _ => StatusCode(500, new { Message = "予期しないエラーが発生しました" })
        };
    }
}
