using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ApplicationController
{
    readonly GetUserProfileService getUserProfileService;
    readonly UpdateUserProfileService updateUserProfileService;

    public UsersController(
        GetUserProfileService getUserProfileService,
        UpdateUserProfileService updateUserProfileService)
    {
        this.getUserProfileService = getUserProfileService;
        this.updateUserProfileService = updateUserProfileService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfile(int id)
    {
        var request = new GetUserProfileRequest { Id = id };
        var response = await getUserProfileService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPut]
    [ValidateModelState]
    public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        await updateUserProfileService.ExecuteAsync(UserId, request);
        return NoContent();
    }
}
