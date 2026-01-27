using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    readonly GetTagsService getTagsService;

    public TagsController(GetTagsService getTagsService)
    {
        this.getTagsService = getTagsService;
    }

    [HttpGet]
    public async Task<ActionResult<GetTagsResponse>> GetTags()
    {
        var request = new GetTagsRequest();
        var response = await getTagsService.ExecuteAsync(request);
        return Ok(response);
    }
}
