using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCoursesByAuthorServiceTest
{
    readonly ICourseRepository courseRepository = Substitute.For<ICourseRepository>();
    readonly UserId ownerId = UserId.From(Guid.CreateVersion7());
    readonly GetCoursesByAuthorService service;

    public GetCoursesByAuthorServiceTest()
    {
        this.service = new GetCoursesByAuthorService(this.courseRepository);
    }

    [Fact]
    public async Task 作者の公開中の講座を返す()
    {
        var articleId = ArticleId.From(Guid.CreateVersion7());
        var summary = new CourseSummary
        {
            Id = CourseId.From(Guid.CreateVersion7()),
            UserId = this.ownerId,
            UserName = "owner",
            Name = "四間飛車講座",
            Articles = [new CourseArticleSummary { Id = articleId, Title = "第1回" }],
            LikeCount = 1,
            PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        this.courseRepository.FindPublishedSummariesByAuthorAsync(this.ownerId, 20, 0)
            .Returns((new List<CourseSummary> { summary }, 1));

        var response = await this.service.ExecuteAsync(new GetCoursesByAuthorRequest { AuthorId = this.ownerId });

        Assert.Equal(this.ownerId, response.AuthorId);
        Assert.Equal("owner", response.AuthorName);
        var dto = Assert.Single(response.Courses);
        Assert.Equal("四間飛車講座", dto.Name);
        Assert.Equal([articleId], dto.ArticleIds);
        Assert.Equal(["第1回"], dto.ArticleNames);
        Assert.Equal(1, dto.LikeCount);
        Assert.True(dto.IsPublished);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task 公開中の講座が無ければ作者名は空になる()
    {
        this.courseRepository.FindPublishedSummariesByAuthorAsync(this.ownerId, 20, 0)
            .Returns((new List<CourseSummary>(), 0));

        var response = await this.service.ExecuteAsync(new GetCoursesByAuthorRequest { AuthorId = this.ownerId });

        Assert.Equal("", response.AuthorName);
        Assert.Empty(response.Courses);
        Assert.Equal(0, response.TotalCount);
    }

    [Fact]
    public async Task 指定した件数と位置でリポジトリから取得する()
    {
        this.courseRepository.FindPublishedSummariesByAuthorAsync(this.ownerId, 10, 30)
            .Returns((new List<CourseSummary>(), 45));

        var response = await this.service.ExecuteAsync(
            new GetCoursesByAuthorRequest { AuthorId = this.ownerId, Limit = 10, Offset = 30 }
        );

        Assert.Equal(45, response.TotalCount);
    }
}
