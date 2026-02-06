using System.Linq.Expressions;
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
            .Include(c => c.Articles.Where(a => a.PublishedAt.HasValue))
            .Include(c => c.CourseLikes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) throw new KeyNotFoundException($"ID: {id} の講座が見つかりません");

        return course;
    }

    public async Task<(List<Course> Courses, int TotalCount)> FindLatestAsync(int limit, int offset)
    {
        var now = DateTime.UtcNow;
        
        var query = context.Courses
            .Include(c => c.User)
            .Include(c => c.CourseLikes)
            .Include(c => c.Articles)
            .Where(c => c.PublishedAt.HasValue && c.PublishedAt.Value <= now)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var courses = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (courses, totalCount);
    }

    public async Task<List<Course>> SearchAsync(string keyword, int limit, int offset)
    {
        var now = DateTime.UtcNow;
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var courses = await context.Courses
            .Include(c => c.User)
            .Include(c => c.CourseLikes)
            .Include(c => c.Articles)
            .Where(searchFilter)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return courses;
    }

    static Expression<Func<Course, bool>> IsPubliclyVisibleAndContainsKeyword(DateTime now, string keyword)
    {
        return c => c.PublishedAt.HasValue && c.PublishedAt.Value <= now &&
                    (c.Name.Contains(keyword) || c.Articles.Any(a => a.Title.Contains(keyword)));
    }
}
