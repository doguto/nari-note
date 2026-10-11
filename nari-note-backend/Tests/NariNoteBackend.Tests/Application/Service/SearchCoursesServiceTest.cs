using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class SearchCoursesServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly SearchCoursesService service;

    public SearchCoursesServiceTest()
    {
        this.service = new SearchCoursesService(this.courseRepository);
    }

    [Fact]
    public async Task キーワードと件数と位置を指定して検索した講座を返す()
    {
        var owner = new UserBuilder().WithName("owner").Build();
        var article = new ArticleBuilder(owner).WithTitle("第1回").Build();
        var course = new CourseBuilder(owner).WithName("四間飛車講座").WithArticles(article).Build();
        this.courseRepository.SearchAsync("四間飛車", 10, 20).Returns(new List<Course> { course });

        var response = await this.service.ExecuteAsync(
            new SearchCoursesRequest { Keyword = "四間飛車", Limit = 10, Offset = 20 }
        );

        var dto = Assert.Single(response.Courses);
        Assert.Equal(course.Id, dto.Id);
        Assert.Equal("四間飛車講座", dto.Name);
        Assert.Equal("owner", dto.UserName);
        Assert.Equal(["第1回"], dto.ArticleNames);
    }
}
