using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class CourseRepository : ICourseRepository
{
    readonly NariNoteDbContext context;
    
    public CourseRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<Course?> FindByIdAsync(CourseId id)
    {
        return await context.Courses.FindAsync(id);
    }

    public async Task<Course> FindForceByIdAsync(CourseId id)
    {
        var course = await FindByIdAsync(id);
        if (course == null) throw new KeyNotFoundException($"ID: {id} の講座が見つかりません");
        
        return course;
    }

    public async Task<Course> CreateAsync(Course entity)
    {
        context.Courses.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Course> UpdateAsync(Course entity)
    {
        context.Courses.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(CourseId id)
    {
        await context.Courses
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<Course> UpdateWithArticlesAsync(Course course)
    {
        context.Courses.Update(course);

        if (course.IsPublished)
        {
            var articles = await context.Articles
                .Where(a => a.CourseId == course.Id && !a.PublishedAt.HasValue)
                .ToListAsync();
            
            foreach (var article in articles)
            {
                article.PublishedAt = course.PublishedAt ?? DateTime.UtcNow;
                article.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> FindByIdWithArticlesAsync(CourseId id)
    {
        var course = await context.Courses
            .Include(c => c.User)
            .Include(c => c.Articles.OrderBy(a => a.ArticleOrder))
            .Include(c => c.CourseLikes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) throw new KeyNotFoundException($"ID: {id} の講座が見つかりません");

        return course;
    }
}
