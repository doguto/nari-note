using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCourseContentServiceTest
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;

    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly User otherUser = new UserBuilder().Build();
    readonly User owner = new UserBuilder().WithName("owner").Build();
    readonly GetCourseContentService service;
    readonly TestTimeProvider timeProvider = new();

    public GetCourseContentServiceTest()
    {
        this.service = new GetCourseContentService(this.courseRepository, this.timeProvider);
    }

    Course SetupCourse(CourseBuilder builder, params Article[] articles)
    {
        var course = builder.Build();
        course.Articles.AddRange(articles);
        this.courseRepository.FindByIdWithArticlesAsync(course.Id).Returns(course);
        this.courseRepository.FindByIdWithAllArticlesAsync(course.Id).Returns(course);
        return course;
    }

    [Fact]
    public async Task 公開中の講座は未ログインでも取得でき_記事は順序どおりに並ぶ()
    {
        var course = SetupCourse(
            new CourseBuilder(this.owner).WithName("四間飛車講座"),
            new ArticleBuilder(this.owner).WithTitle("第2回").InCourse(null!, 2).Build(),
            new ArticleBuilder(this.owner).WithTitle("第1回").InCourse(null!, 1).Build()
        );

        var response = await this.service.ExecuteAsync(new GetCourseContentRequest { Id = course.Id });

        Assert.Equal("四間飛車講座", response.Name);
        Assert.Equal("owner", response.UserName);
        Assert.Equal(["第1回", "第2回"], response.Articles.Select(a => a.Title));
    }

    [Fact]
    public async Task 下書きの講座は作成者以外には存在しないものとして扱う()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).Draft());
        var request = new GetCourseContentRequest { Id = course.Id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request, this.otherUser.Id));

        var response = await this.service.ExecuteAsync(request, this.owner.Id);
        Assert.False(response.IsPublished);
    }

    [Fact]
    public async Task 予約公開の講座は公開日時になるまで作成者以外は取得できない()
    {
        var publishAt = Now.AddHours(1);
        var course = SetupCourse(new CourseBuilder(this.owner).PublishedAt(publishAt));
        var request = new GetCourseContentRequest { Id = course.Id };

        this.timeProvider.SetUtcNow(publishAt.AddTicks(-1));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.service.ExecuteAsync(request));

        this.timeProvider.SetUtcNow(publishAt);
        Assert.Equal(course.Id, (await this.service.ExecuteAsync(request)).Id);
    }

    [Fact]
    public async Task 編集用の取得は作成者だけに許可する()
    {
        var course = SetupCourse(new CourseBuilder(this.owner).Draft());
        var request = new GetCourseContentRequest { Id = course.Id };

        await Assert.ThrowsAsync<ForbiddenException>(
            () => this.service.ExecuteForEditAsync(this.otherUser.Id, request)
        );

        Assert.Equal(course.Id, (await this.service.ExecuteForEditAsync(this.owner.Id, request)).Id);
    }
}
