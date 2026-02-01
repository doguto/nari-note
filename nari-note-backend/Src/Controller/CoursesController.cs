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
    readonly UpdateCourseService updateCourseService;

    public CoursesController(CreateCourseService createCourseService, UpdateCourseService updateCourseService)
    {
        this.createCourseService = createCourseService;
        this.updateCourseService = updateCourseService;
    }

    [HttpPost]
    [RequireAuth]
    [ValidateModelState]
    public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var response = await createCourseService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
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
