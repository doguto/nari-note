using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCoursesServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly GetCoursesService service;

    public GetCoursesServiceTest()
    {
        this.service = new GetCoursesService(this.courseRepository);
    }

    [Fact]
    public async Task 指定した件数と位置で取得した講座を返す()
    {
        var owner = new UserBuilder().WithName("owner").WithProfileImage("https://images.test/owner").Build();
        var liker = new UserBuilder().Build();
        var article = new ArticleBuilder(owner).WithTitle("第1回").Build();
        var course = new CourseBuilder(owner).WithName("四間飛車講座").WithArticles(article).LikedBy(liker).Build();
        this.courseRepository.FindLatestAsync(10, 20).Returns((new List<Course> { course }, 1));

        var response = await this.service.ExecuteAsync(new GetCoursesRequest { Limit = 10, Offset = 20 });

        var dto = Assert.Single(response.Courses);
        Assert.Equal(course.Id, dto.Id);
        Assert.Equal("四間飛車講座", dto.Name);
        Assert.Equal(owner.Id, dto.UserId);
        Assert.Equal("owner", dto.UserName);
        Assert.Equal("https://images.test/owner", dto.UserIconImageUrl);
        Assert.Equal([article.Id], dto.ArticleIds);
        Assert.Equal(["第1回"], dto.ArticleNames);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(TestTimeProvider.DefaultUtcNow.AddDays(-1), dto.PublishedAt);
    }
}
