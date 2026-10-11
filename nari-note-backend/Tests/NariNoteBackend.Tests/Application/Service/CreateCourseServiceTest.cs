using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class CreateCourseServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly CreateCourseService service;

    public CreateCourseServiceTest()
    {
        this.service = new CreateCourseService(this.courseRepository);
    }

    [Fact]
    public async Task ログインユーザーを作成者として下書きの講座を作成する()
    {
        var owner = new UserBuilder().Build();
        var courseId = CourseId.From(Guid.CreateVersion7());
        this.courseRepository
            .CreateAsync(Arg.Any<Course>())
            .Returns(call =>
            {
                var course = call.Arg<Course>();
                course.Id = courseId;
                return course;
            });

        var response = await this.service.ExecuteAsync(owner.Id, new CreateCourseRequest { Name = "四間飛車講座" });

        await this.courseRepository.Received(1).CreateAsync(
            Arg.Is<Course>(c => c.UserId == owner.Id && c.Name == "四間飛車講座" && c.PublishedAt == null)
        );
        Assert.Equal(courseId, response.Course.Id);
        Assert.Equal(owner.Id, response.Course.UserId);
        Assert.Equal("四間飛車講座", response.Course.Name);
        Assert.False(response.Course.IsPublished);
        Assert.Null(response.Course.PublishedAt);
    }
}
