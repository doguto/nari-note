using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class DeleteCourseServiceTest
{
    readonly Course course;
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly DeleteCourseService service;

    public DeleteCourseServiceTest()
    {
        this.course = new CourseBuilder(this.owner).Build();
        this.courseRepository.FindForceByIdAsync(this.course.Id).Returns(this.course);
        this.service = new DeleteCourseService(this.courseRepository);
    }

    [Fact]
    public async Task 作成者は講座を削除できる()
    {
        await this.service.ExecuteAsync(this.owner.Id, new DeleteCourseRequest { Id = this.course.Id });

        await this.courseRepository.Received(1).DeleteAsync(this.course.Id);
    }

    [Fact]
    public async Task 作成者以外は講座を削除できない()
    {
        var otherUser = new UserBuilder().Build();
        var request = new DeleteCourseRequest { Id = this.course.Id };

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(otherUser.Id, request));

        await this.courseRepository.DidNotReceiveWithAnyArgs().DeleteAsync(this.course.Id);
    }
}
