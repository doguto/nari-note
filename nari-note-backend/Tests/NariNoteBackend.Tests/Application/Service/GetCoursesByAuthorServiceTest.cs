using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCoursesByAuthorServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly User owner = new UserBuilder().WithName("owner").Build();
    readonly GetCoursesByAuthorService service;

    public GetCoursesByAuthorServiceTest()
    {
        this.service = new GetCoursesByAuthorService(this.courseRepository);
    }

    [Fact]
    public async Task 作者の公開中の講座を返す()
    {
        var liker = new UserBuilder().Build();
        var article = new ArticleBuilder(this.owner).WithTitle("第1回").Build();
        var course = new CourseBuilder(this.owner).WithName("四間飛車講座").WithArticles(article).LikedBy(liker).Build();
        this.courseRepository.FindPublishedByAuthorAsync(this.owner.Id).Returns(new List<Course> { course });

        var response = await this.service.ExecuteAsync(new GetCoursesByAuthorRequest { AuthorId = this.owner.Id });

        Assert.Equal(this.owner.Id, response.AuthorId);
        Assert.Equal("owner", response.AuthorName);
        var dto = Assert.Single(response.Courses);
        Assert.Equal("四間飛車講座", dto.Name);
        Assert.Equal(["第1回"], dto.ArticleNames);
        Assert.Equal(1, dto.LikeCount);
    }

    [Fact]
    public async Task 公開中の講座が無ければ作者名は空になる()
    {
        this.courseRepository.FindPublishedByAuthorAsync(this.owner.Id).Returns(new List<Course>());

        var response = await this.service.ExecuteAsync(new GetCoursesByAuthorRequest { AuthorId = this.owner.Id });

        Assert.Equal("", response.AuthorName);
        Assert.Empty(response.Courses);
    }
}
