using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class UpdateCourseServiceTest
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly UpdateCourseService service;

    public UpdateCourseServiceTest()
    {
        this.service = new UpdateCourseService(this.courseRepository, new TestTimeProvider());
    }

    Course SetupCourse(CourseBuilder builder)
    {
        var course = builder.Build();
        this.courseRepository.FindForceByIdAsync(course.Id).Returns(course);
        return course;
    }

    [Fact]
    public async Task 講座名を更新し更新日時を現在時刻にする()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).WithName("旧講座名"));

        var response = await this.service.ExecuteAsync(
            this.owner.Id,
            new UpdateCourseRequest { Id = course.Id, Name = "新講座名" }
        );

        Assert.Equal("新講座名", course.Name);
        Assert.Equal(Now, course.UpdatedAt);
        Assert.Equal(Now, response.UpdatedAt);
        await this.courseRepository.Received(1).UpdateWithArticlesAsync(course);
    }

    [Fact]
    public async Task 下書きを公開すると現在時刻が公開日時になる()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).Draft());

        await this.service.ExecuteAsync(this.owner.Id, new UpdateCourseRequest { Id = course.Id, IsPublished = true });

        Assert.Equal(Now, course.PublishedAt);
    }

    [Fact]
    public async Task 公開済みの講座を更新しても公開日時は変わらない()
    {
        var publishedAt = Now.AddDays(-5);
        var course = SetupCourse(new CourseBuilder(this.owner).PublishedAt(publishedAt));

        await this.service.ExecuteAsync(
            this.owner.Id,
            new UpdateCourseRequest { Id = course.Id, Name = "新講座名", IsPublished = true }
        );

        Assert.Equal(publishedAt, course.PublishedAt);
    }

    [Fact]
    public async Task 公開日時が指定されていればその日時に変更する()
    {
        var scheduledAt = Now.AddDays(3);
        var course = SetupCourse(new CourseBuilder(this.owner).Draft());

        await this.service.ExecuteAsync(
            this.owner.Id,
            new UpdateCourseRequest { Id = course.Id, PublishedAt = scheduledAt }
        );

        Assert.Equal(scheduledAt, course.PublishedAt);
    }

    [Fact]
    public async Task 作成者以外は更新できない()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).WithName("講座名"));
        var otherUserId = UserId.From(Guid.CreateVersion7());
        var request = new UpdateCourseRequest { Id = course.Id, Name = "書き換え" };

        await Assert.ThrowsAsync<ForbiddenException>(() => this.service.ExecuteAsync(otherUserId, request));

        Assert.Equal("講座名", course.Name);
        await this.courseRepository.DidNotReceive().UpdateWithArticlesAsync(Arg.Any<Course>());
    }
}
