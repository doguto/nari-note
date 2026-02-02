using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ApplicationController
{
    readonly CreateCourseService createCourseService;
    readonly DeleteCourseService deleteCourseService;
    readonly UpdateCourseService updateCourseService;
    readonly GetCoursesService getCoursesService;
    readonly GetCourseContentService getCourseContentService;

    public CoursesController(
        CreateCourseService createCourseService,
        DeleteCourseService deleteCourseService,
        UpdateCourseService updateCourseService,
        GetCoursesService getCoursesService,
        GetCourseContentService getCourseContentService
    )
    {
        this.createCourseService = createCourseService;
        this.deleteCourseService = deleteCourseService;
        this.updateCourseService = updateCourseService;
        this.getCourseContentService = getCourseContentService;
        this.getCoursesService = getCoursesService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<GetCoursesResponse>> GetCourses(
        [FromQuery] int limit = 20, [FromQuery] int offset = 0
    )
    {
        var request = new GetCoursesRequest { Limit = limit, Offset = offset };
        var response = await getCoursesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost]
    [RequireAuth]
    [ValidateModelState]
    public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var response = await createCourseService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<GetCourseContentResponse>> GetCourse(CourseId id)
    {
        var request = new GetCourseContentRequest { Id = id };
        var response = await getCourseContentService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [RequireAuth]
    public async Task<ActionResult> DeleteCourse(CourseId id)
    {
        var request = new DeleteCourseRequest { Id = id };
        await deleteCourseService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpPut("{id}")]
    [RequireAuth]
    [ValidateModelState]
    public async Task<ActionResult<UpdateCourseResponse>> UpdateCourse(CourseId id, [FromBody] UpdateCourseRequest request)
    {
        request.Id = id;
        var response = await updateCourseService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }
}
