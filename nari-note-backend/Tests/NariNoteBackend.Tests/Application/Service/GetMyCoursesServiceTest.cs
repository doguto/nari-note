using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetMyCoursesServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly GetMyCoursesService service;

    public GetMyCoursesServiceTest()
    {
        this.service = new GetMyCoursesService(this.courseRepository);
    }

    [Fact]
    public async Task 自分の講座を下書きも含めて返す()
    {
        var owner = new UserBuilder().WithName("owner").Build();
        var draftArticle = new ArticleBuilder(owner).WithTitle("下書き記事").Draft().Build();
        var published = new CourseBuilder(owner).WithName("公開中の講座").Build();
        var draft = new CourseBuilder(owner).WithName("下書きの講座").Draft().WithArticles(draftArticle).Build();
        this.courseRepository.FindAllByAuthorAsync(owner.Id).Returns(new List<Course> { published, draft });

        var response = await this.service.ExecuteAsync(owner.Id);

        Assert.Equal(["公開中の講座", "下書きの講座"], response.Courses.Select(c => c.Name));
        Assert.Equal([true, false], response.Courses.Select(c => c.IsPublished));
        Assert.Equal("owner", response.Courses[0].UserName);
        Assert.Equal(["下書き記事"], response.Courses[1].ArticleNames);
    }
}
