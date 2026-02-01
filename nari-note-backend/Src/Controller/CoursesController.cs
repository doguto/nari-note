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

    public CoursesController(
        CreateCourseService createCourseService,
        DeleteCourseService deleteCourseService)
    {
        this.createCourseService = createCourseService;
        this.deleteCourseService = deleteCourseService;
    }

    [HttpPost]
    [RequireAuth]
    [ValidateModelState]
    public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var response = await createCourseService.ExecuteAsync(UserId!.Value, request);
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
}
