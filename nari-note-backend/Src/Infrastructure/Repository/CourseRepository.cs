using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
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

    public async Task<Course> UpdateWithArticlesAsync(Course course, DateTime? previousPublishedAt)
    {
        context.Courses.Update(course);

        if (course.IsPublished)
        {
            var now = timeProvider.UtcNow();
            // 下書きの記事に加え、講座の予約公開に合わせて予約されたままの記事も講座の公開日時に揃える
            var articles = await context.Articles
                                        .Where(
                                            a => a.CourseId == course.Id
                                                 && (!a.PublishedAt.HasValue
                                                     || (a.PublishedAt == previousPublishedAt && a.PublishedAt > now))
                                        )
                                        .ToListAsync();

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
                           .Include(c => c.Articles.Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now))
                           .Where(c => c.PublishedAt.HasValue && c.PublishedAt.Value <= now)
                           .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var courses = await query
                            .Skip(offset)
                            .Take(limit)
                            .ToListAsync();

        return (courses, totalCount);
    }

    public async Task<(List<Course> Courses, int TotalCount)> SearchAsync(string keyword, int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var totalCount = await context.Courses.Where(searchFilter).CountAsync();
        var courses = await context.Courses
                                   .Include(c => c.User)
                                   .Include(c => c.CourseLikes)
                                   .Include(
                                       c => c.Articles.Where(
                                           a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now
                                       )
                                   )
                                   .Where(searchFilter)
                                   .OrderByDescending(c => c.CreatedAt)
                                   .ThenByDescending(c => c.Id)
                                   .Skip(offset)
                                   .Take(limit)
                                   .ToListAsync();

        return (courses, totalCount);
    }

    public async Task<(List<CourseSummary> Courses, int TotalCount)> FindPublishedSummariesByAuthorAsync(UserId authorId, int limit, int offset)
    {
        var now = timeProvider.UtcNow();

        var query = context.Courses
                           .Where(c => c.UserId == authorId && c.PublishedAt.HasValue && c.PublishedAt.Value <= now);

        var totalCount = await query.CountAsync();
        // 予約投稿（公開日時が未来）の記事は含めない。記事は本文を読み込まず ID とタイトルだけを取得する
        var courses = await query.OrderByDescending(c => c.CreatedAt)
                                 .ThenByDescending(c => c.Id)
                                 .Skip(offset)
                                 .Take(limit)
                                 .Select(c => new CourseSummary
                                 {
                                     Id = c.Id,
                                     UserId = c.UserId,
                                     UserName = c.User.Name,
                                     UserProfileImage = c.User.ProfileImage,
                                     Name = c.Name,
                                     Articles = c.Articles
                                                 .Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now)
                                                 .OrderBy(a => a.ArticleOrder)
                                                 .ThenBy(a => a.Id)
                                                 .Select(a => new CourseArticleSummary { Id = a.Id, Title = a.Title })
                                                 .ToList(),
                                     LikeCount = c.CourseLikes.Count(),
                                     PublishedAt = c.PublishedAt
                                 })
                                 .ToListAsync();

        return (courses, totalCount);
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
                    (c.Name.Contains(keyword) ||
                     c.Articles.Any(
                         a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now &&
                              (a.Title.Contains(keyword) || a.Body.Contains(keyword))
                     ));
    }
}
