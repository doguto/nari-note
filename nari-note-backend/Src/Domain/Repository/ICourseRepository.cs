using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ICourseRepository : IRepository<Course, CourseId>
{
    Task<Course> UpdateWithArticlesAsync(Course course);
    Task<Course> FindByIdWithArticlesAsync(CourseId id);
    Task<(List<Course> Courses, int TotalCount)> FindLatestAsync(int limit, int offset);
    Task<List<Course>> SearchAsync(string keyword, int limit, int offset);
}
