using System.ComponentModel.DataAnnotations;
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
    readonly UploadUserIconService uploadUserIconService;

    public UsersController(
        GetUserProfileService getUserProfileService,
        UpdateUserProfileService updateUserProfileService,
        ToggleFollowService toggleFollowService,
        GetFollowersService getFollowersService,
        GetFollowingsService getFollowingsService,
        GetLikedArticlesService getLikedArticlesService,
        UploadUserIconService uploadUserIconService
    )
    {
        this.getUserProfileService = getUserProfileService;
        this.updateUserProfileService = updateUserProfileService;
        this.toggleFollowService = toggleFollowService;
        this.getFollowersService = getFollowersService;
        this.getFollowingsService = getFollowingsService;
        this.getLikedArticlesService = getLikedArticlesService;
        this.uploadUserIconService = uploadUserIconService;
    }

    [HttpGet("{id}")]
    [Auth(AuthLevel.Optional)]
    public async Task<ActionResult> GetUserProfile(UserId id)
    {
        var request = new GetUserProfileRequest { Id = id };

        // 認証済みの場合は現在のユーザーIDを渡す
        var response = await getUserProfileService.ExecuteAsync(request, UserId);
        return Ok(response);
    }

    [HttpPost("icon")]
    [Auth(AuthLevel.VerifiedEmail)]
    [NoTransaction]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult> UploadUserIcon(IFormFile file)
    {
        var response = await uploadUserIconService.ExecuteAsync(UserId!.Value, file);
        return Ok(response);
    }

    [HttpPut]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        var response = await updateUserProfileService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/follow")]
    [Auth(AuthLevel.VerifiedEmail)]
    public async Task<ActionResult> ToggleFollow(UserId id)
    {
        var request = new ToggleFollowRequest
        {
            FollowingId = id
        };
        var response = await toggleFollowService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("{id}/followers")]
    [Auth(AuthLevel.Anonymous)]
    [ValidateModelState]
    public async Task<ActionResult> GetFollowers(
        UserId id,
        [FromQuery, Range(1, 100, ErrorMessage = "取得件数は1から100の範囲で指定してください")] int limit = 20,
        [FromQuery, Range(0, int.MaxValue, ErrorMessage = "オフセットは0以上の値を指定してください")] int offset = 0
    )
    {
        var request = new GetFollowersRequest { UserId = id, Limit = limit, Offset = offset };
        var response = await getFollowersService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}/followings")]
    [Auth(AuthLevel.Anonymous)]
    [ValidateModelState]
    public async Task<ActionResult> GetFollowings(
        UserId id,
        [FromQuery, Range(1, 100, ErrorMessage = "取得件数は1から100の範囲で指定してください")] int limit = 20,
        [FromQuery, Range(0, int.MaxValue, ErrorMessage = "オフセットは0以上の値を指定してください")] int offset = 0
    )
    {
        var request = new GetFollowingsRequest { UserId = id, Limit = limit, Offset = offset };
        var response = await getFollowingsService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}/liked-articles")]
    [Auth(AuthLevel.Anonymous)]
    [ValidateModelState]
    public async Task<ActionResult> GetLikedArticles(
        UserId id,
        [FromQuery, Range(1, 100, ErrorMessage = "取得件数は1から100の範囲で指定してください")] int limit = 20,
        [FromQuery, Range(0, int.MaxValue, ErrorMessage = "オフセットは0以上の値を指定してください")] int offset = 0
    )
    {
        var request = new GetLikedArticlesRequest { UserId = id, Limit = limit, Offset = offset };
        var response = await getLikedArticlesService.ExecuteAsync(request);
        return Ok(response);
    }
}
