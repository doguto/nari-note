using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ICourseRepository : IRepository<Course, CourseId>
{
    Task<Course> UpdateWithArticlesAsync(Course course);
}
