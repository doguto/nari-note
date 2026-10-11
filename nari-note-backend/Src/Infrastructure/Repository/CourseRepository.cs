using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class CourseRepository : ICourseRepository
{
    readonly NariNoteDbContext context;
    readonly TimeProvider timeProvider;

    public CourseRepository(NariNoteDbContext context, TimeProvider timeProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
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

            var now = timeProvider.UtcNow();
            foreach (var article in articles)
            {
                article.PublishedAt = course.PublishedAt ?? now;
                article.UpdatedAt = now;
            }
        }

        await context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> FindByIdWithArticlesAsync(CourseId id)
    {
        // 予約投稿（公開日時が未来）の記事は含めない
        var now = timeProvider.UtcNow();
        var course = await context.Courses
                                  .Include(c => c.User)
                                  .Include(c => c.Articles.Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now))
                                  .Include(c => c.CourseLikes)
                                  .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) throw new KeyNotFoundException($"ID: {id} の講座が見つかりません");

        return course;
    }

    public async Task<Course> FindByIdWithAllArticlesAsync(CourseId id)
    {
        var course = await context.Courses
                                  .Include(c => c.User)
                                  .Include(c => c.Articles)
                                  .Include(c => c.CourseLikes)
                                  .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) throw new KeyNotFoundException($"ID: {id} の講座が見つかりません");

        return course;
    }

    public async Task<(List<Course> Courses, int TotalCount)> FindLatestAsync(int limit, int offset)
    {
        var now = timeProvider.UtcNow();

        var query = context.Courses
                           .Include(c => c.User)
                           .Include(c => c.CourseLikes)
                           .Include(c => c.Articles.Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value < now))
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
        var now = timeProvider.UtcNow();
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var courses = await context.Courses
                                   .Include(c => c.User)
                                   .Include(c => c.CourseLikes)
                                   .Include(
                                       c => c.Articles.Where(
                                           a => a.PublishedAt.HasValue && a.PublishedAt.Value < now
                                       )
                                   )
                                   .Where(searchFilter)
                                   .OrderByDescending(c => c.CreatedAt)
                                   .Skip(offset)
                                   .Take(limit)
                                   .ToListAsync();

        return courses;
    }

    public async Task<List<Course>> FindPublishedByAuthorAsync(UserId authorId)
    {
        var now = timeProvider.UtcNow();

        return await context.Courses
                            .Include(c => c.User)
                            .Include(c => c.CourseLikes)
                            .Include(c => c.Articles)
                            .Where(c => c.UserId == authorId && c.PublishedAt.HasValue && c.PublishedAt.Value <= now)
                            .OrderByDescending(c => c.CreatedAt)
                            .ToListAsync();
    }

    public async Task<List<Course>> FindAllByAuthorAsync(UserId authorId)
    {
        return await context.Courses
                            .Include(c => c.User)
                            .Include(c => c.CourseLikes)
                            .Include(c => c.Articles)
                            .Where(c => c.UserId == authorId)
                            .OrderByDescending(c => c.CreatedAt)
                            .ToListAsync();
    }

    static Expression<Func<Course, bool>> IsPubliclyVisibleAndContainsKeyword(DateTime now, string keyword)
    {
        return c => c.PublishedAt.HasValue && c.PublishedAt.Value <= now &&
                    (c.Name.Contains(keyword) || c.Articles.Any(a => a.Title.Contains(keyword)));
    }
}
