using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ICourseRepository : IRepository<Course, CourseId>
{
    Task<Course> UpdateWithArticlesAsync(Course course, DateTime? previousPublishedAt);
    Task<Course> FindByIdWithArticlesAsync(CourseId id);
    Task<Course> FindByIdWithAllArticlesAsync(CourseId id);
    Task<(List<Course> Courses, int TotalCount)> FindLatestAsync(int limit, int offset);
    Task<(List<Course> Courses, int TotalCount)> SearchAsync(string keyword, int limit, int offset);
    Task<(List<CourseSummary> Courses, int TotalCount)> FindPublishedSummariesByAuthorAsync(UserId authorId, int limit, int offset);
    Task<List<Course>> FindAllByAuthorAsync(UserId authorId);
}
