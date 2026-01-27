using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    readonly GetPopularTagsService getPopularTagsService;

    public TagsController(GetPopularTagsService getPopularTagsService)
    {
        this.getPopularTagsService = getPopularTagsService;
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<GetPopularTagsResponse>> GetPopularTags()
    {
        var request = new GetPopularTagsRequest();
        var response = await getPopularTagsService.ExecuteAsync(request);
        return Ok(response);
    }
}
