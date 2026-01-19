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
    readonly GetFollowersService getFollowersService;
    readonly GetFollowingsService getFollowingsService;
    readonly GetLikedArticlesService getLikedArticlesService;
    readonly GetUserProfileService getUserProfileService;
    readonly ToggleFollowService toggleFollowService;
    readonly UpdateUserProfileService updateUserProfileService;

    public UsersController(
        GetUserProfileService getUserProfileService,
        UpdateUserProfileService updateUserProfileService,
        ToggleFollowService toggleFollowService,
        GetFollowersService getFollowersService,
        GetFollowingsService getFollowingsService,
        GetLikedArticlesService getLikedArticlesService
    )
    {
        this.getUserProfileService = getUserProfileService;
        this.updateUserProfileService = updateUserProfileService;
        this.toggleFollowService = toggleFollowService;
        this.getFollowersService = getFollowersService;
        this.getFollowingsService = getFollowingsService;
        this.getLikedArticlesService = getLikedArticlesService;
    }

    [HttpGet("{id}")]
    [OptionalAuth]
    public async Task<ActionResult> GetUserProfile(UserId id)
    {
        var request = new GetUserProfileRequest { Id = id };

        // 認証済みの場合は現在のユーザーIDを渡す
        var response = await getUserProfileService.ExecuteAsync(request, NullableUserId);
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

    [HttpGet("{id}/followers")]
    public async Task<ActionResult> GetFollowers(UserId id)
    {
        var request = new GetFollowersRequest { UserId = id };
        var response = await getFollowersService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}/followings")]
    public async Task<ActionResult> GetFollowings(UserId id)
    {
        var request = new GetFollowingsRequest { UserId = id };
        var response = await getFollowingsService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}/liked-articles")]
    public async Task<ActionResult> GetLikedArticles(UserId id)
    {
        var request = new GetLikedArticlesRequest { UserId = id };
        var response = await getLikedArticlesService.ExecuteAsync(request);
        return Ok(response);
    }
}
