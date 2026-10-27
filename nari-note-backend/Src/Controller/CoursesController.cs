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
    readonly GetCourseContentService getCourseContentService;
    readonly GetCourseContentForEditService getCourseContentForEditService;
    readonly GetCoursesService getCoursesService;
    readonly GetCoursesByAuthorService getCoursesByAuthorService;
    readonly GetMyCoursesService getMyCoursesService;
    readonly SearchCoursesService searchCoursesService;
    readonly UpdateCourseService updateCourseService;

    public CoursesController(
        CreateCourseService createCourseService,
        DeleteCourseService deleteCourseService,
        UpdateCourseService updateCourseService,
        GetCoursesService getCoursesService,
        GetCoursesByAuthorService getCoursesByAuthorService,
        GetMyCoursesService getMyCoursesService,
        GetCourseContentService getCourseContentService,
        GetCourseContentForEditService getCourseContentForEditService,
        SearchCoursesService searchCoursesService
    )
    {
        this.createCourseService = createCourseService;
        this.deleteCourseService = deleteCourseService;
        this.updateCourseService = updateCourseService;
        this.getCourseContentService = getCourseContentService;
        this.getCourseContentForEditService = getCourseContentForEditService;
        this.getCoursesService = getCoursesService;
        this.getCoursesByAuthorService = getCoursesByAuthorService;
        this.getMyCoursesService = getMyCoursesService;
        this.searchCoursesService = searchCoursesService;
    }

    [HttpGet]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult<GetCoursesResponse>> GetCourses(
        [FromQuery] int limit = 20, [FromQuery] int offset = 0
    )
    {
        var request = new GetCoursesRequest { Limit = limit, Offset = offset };
        var response = await getCoursesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Auth(AuthLevel.Anonymous)]
    [ValidateModelState]
    public async Task<ActionResult<SearchCoursesResponse>> SearchCourses([FromQuery] SearchCoursesRequest request)
    {
        var response = await searchCoursesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("my")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetMyCoursesResponse>> GetMyCourses()
    {
        var response = await getMyCoursesService.ExecuteAsync(UserId!.Value);
        return Ok(response);
    }

    [HttpGet("author/{authorId}")]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult<GetCoursesByAuthorResponse>> GetCoursesByAuthor(
        UserId authorId, [FromQuery] int limit = 20, [FromQuery] int offset = 0
    )
    {
        var request = new GetCoursesByAuthorRequest { AuthorId = authorId, Limit = limit, Offset = offset };
        var response = await getCoursesByAuthorService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost]
    [Auth(AuthLevel.VerifiedEmail)]
    [ValidateModelState]
    public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var response = await createCourseService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Auth(AuthLevel.Optional)]
    public async Task<ActionResult<GetCourseContentResponse>> GetCourseContent(CourseId id)
    {
        var request = new GetCourseContentRequest { Id = id };
        var response = await getCourseContentService.ExecuteAsync(request, UserId);
        return Ok(response);
    }

    [HttpGet("{id}/for-edit")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetCourseContentForEditResponse>> GetCourseContentForEdit(CourseId id)
    {
        var request = new GetCourseContentRequest { Id = id };
        var response = await getCourseContentForEditService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Auth(AuthLevel.VerifiedEmail)]
    public async Task<ActionResult> DeleteCourse(CourseId id)
    {
        var request = new DeleteCourseRequest { Id = id };
        await deleteCourseService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Auth(AuthLevel.VerifiedEmail)]
    [ValidateModelState]
    public async Task<ActionResult<UpdateCourseResponse>> UpdateCourse(CourseId id, [FromBody] UpdateCourseRequest request)
    {
        request.Id = id;
        var response = await updateCourseService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }
}
