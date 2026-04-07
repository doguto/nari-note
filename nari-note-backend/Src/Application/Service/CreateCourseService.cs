using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class CreateCourseService
{
    readonly ICourseRepository courseRepository;

    public CreateCourseService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<CreateCourseResponse> ExecuteAsync(UserId userId, CreateCourseRequest request)
    {
        var course = new Course
        {
            UserId = userId,
            Name = request.Name
        };
        course = await courseRepository.CreateAsync(course);

        var courseDto = new CourseDto
        {
            Id = course.Id,
            UserId = userId,
            Name = course.Name,
            IsPublished = course.IsPublished,
            PublishedAt = course.PublishedAt
        };

        return new CreateCourseResponse { Course = courseDto };
    }
}
