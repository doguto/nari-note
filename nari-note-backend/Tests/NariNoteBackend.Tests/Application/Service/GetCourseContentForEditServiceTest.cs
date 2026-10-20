using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCourseContentForEditServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly User otherUser = new UserBuilder().Build();
    readonly User owner = new UserBuilder().WithName("owner").Build();
    readonly GetCourseContentForEditService service;

    public GetCourseContentForEditServiceTest()
    {
        this.service = new GetCourseContentForEditService(this.courseRepository);
    }

    Course SetupCourse(CourseBuilder builder)
    {
        var course = builder.Build();
        this.courseRepository.FindByIdWithAllArticlesAsync(course.Id).Returns(course);
        return course;
    }

    [Fact]
    public async Task 編集用の取得は作成者だけに許可する()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).Draft());
        var request = new GetCourseContentRequest { Id = course.Id };

        await Assert.ThrowsAsync<ForbiddenException>(
            () => this.service.ExecuteAsync(this.otherUser.Id, request)
        );

        Assert.Equal(course.Id, (await this.service.ExecuteAsync(this.owner.Id, request)).Id);
    }
}
