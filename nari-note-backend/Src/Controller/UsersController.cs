using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ApplicationController
{
    readonly GetUserProfileService getUserProfileService;
    readonly UpdateUserProfileService updateUserProfileService;
    readonly ToggleFollowService toggleFollowService;

    public UsersController(
        GetUserProfileService getUserProfileService,
        UpdateUserProfileService updateUserProfileService,
        ToggleFollowService toggleFollowService)
    {
        this.getUserProfileService = getUserProfileService;
        this.updateUserProfileService = updateUserProfileService;
        this.toggleFollowService = toggleFollowService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfile(UserId id)
    {
        var request = new GetUserProfileRequest { Id = id };
        var response = await getUserProfileService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPut]
    [ValidateModelState]
    public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        var response = await updateUserProfileService.ExecuteAsync(UserId, request);
        return Ok(response);
    }

    [HttpPost("{id}/follow")]
    public async Task<ActionResult> ToggleFollow(UserId id)
    {
        var request = new ToggleFollowRequest 
        { 
            FollowingId = id
        };
        var response = await toggleFollowService.ExecuteAsync(UserId, request);
        return Ok(response);
    }
}
