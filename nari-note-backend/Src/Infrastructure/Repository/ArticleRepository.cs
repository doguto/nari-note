using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    readonly NariNoteDbContext context;

    public ArticleRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<Article> CreateAsync(Article article)
    {
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }

    public async Task<Article?> FindByIdAsync(ArticleId id)
    {
        return await context.Articles
                            .Include(a => a.Author)
                            .Include(a => a.ArticleTags)
                            .ThenInclude(at => at.Tag)
                            .Include(a => a.Likes)
                            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article> FindForceByIdAsync(ArticleId id)
    {
        var article = await FindByIdAsync(id);
        if (article == null) throw new KeyNotFoundException($"記事{id}が存在しません");

        return article;
    }

    public async Task<Article> UpdateAsync(Article entity)
    {
        context.Articles.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<Article>> FindByAuthorAsync(UserId authorId)
    {
        return await context.Articles
                            .Include(a => a.Author)
                            .Include(a => a.ArticleTags)
                            .ThenInclude(at => at.Tag)
                            .Include(a => a.Likes)
                            .Where(a => a.AuthorId == authorId)
                            .OrderByDescending(a => a.CreatedAt)
                            .ToListAsync();
    }

    public async Task<List<Article>> FindByTagAsync(string tagName)
    {
        var now = DateTime.UtcNow;
        var visibilityFilter = IsPubliclyVisible(now);

        return await context.Articles
                            .Include(a => a.Author)
                            .Include(a => a.ArticleTags)
                            .ThenInclude(at => at.Tag)
                            .Include(a => a.Likes)
                            .Where(a => a.ArticleTags.Any(at => EF.Functions.ILike(at.Tag.Name, tagName)))
                            .Where(visibilityFilter)
                            .OrderByDescending(a => a.CreatedAt)
                            .ToListAsync();
    }

    public async Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null)
    {
        context.Articles.Update(article);

        if (tagNames != null)
        {
            // Remove existing ArticleTags
            var existingArticleTags = await context.ArticleTags
                                                   .Where(at => at.ArticleId == article.Id)
                                                   .ToListAsync();
            context.ArticleTags.RemoveRange(existingArticleTags);

            if (tagNames.Count > 0)
            {
                var existingTags = await context.Tags
                                                .Where(t => tagNames.Contains(t.Name))
                                                .ToListAsync();

                var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();
                var newTagNames = tagNames.Where(tn => !existingTagNames.Contains(tn)).ToList();

                // Create new tags
                var newTags = newTagNames.Select(name => new Tag { Name = name }).ToList();

                if (newTags.Count > 0)
                {
                    context.Tags.AddRange(newTags);
                    await context.SaveChangesAsync(); // Save to get Tag IDs
                }

                // Combine all tags
                var allTags = existingTags.Concat(newTags).ToList();

                // Create ArticleTag associations
                var articleTags = allTags.Select(tag => new ArticleTag
                {
                    ArticleId = article.Id,
                    TagId = tag.Id,
                    CreatedAt = DateTime.UtcNow,
                    Article = article,
                    Tag = tag
                }).ToList();

                context.ArticleTags.AddRange(articleTags);
            }
        }

        await context.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(ArticleId id)
    {
        var article = await context.Articles.FindAsync(id);
        if (article != null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }
    }

    public async Task<(List<Article> Articles, int TotalCount)> FindLatestAsync(int limit, int offset)
    {
        var now = DateTime.UtcNow;
        var visibilityFilter = IsPubliclyVisible(now);

        var query = context.Articles
                           .Include(a => a.Author)
                           .Include(a => a.ArticleTags)
                           .ThenInclude(at => at.Tag)
                           .Include(a => a.Likes)
                           .Where(visibilityFilter)
                           .OrderByDescending(a => a.CreatedAt);

        // 注: ページネーションの標準的な実装として、
        // 総数取得とデータ取得を別々に実行しています。
        // 大量データがある場合は、キャッシュの利用を検討してください。
        var totalCount = await query.CountAsync();
        var articles = await query
                             .Skip(offset)
                             .Take(limit)
                             .ToListAsync();

        return (articles, totalCount);
    }

    public async Task<List<Article>> FindDraftsByAuthorAsync(UserId authorId)
    {
        return await context.Articles
                            .Include(a => a.Author)
                            .Include(a => a.ArticleTags)
                            .ThenInclude(at => at.Tag)
                            .Where(a => a.AuthorId == authorId && !a.IsPublished)
                            .OrderByDescending(a => a.UpdatedAt)
                            .ToListAsync();
    }

    public async Task<List<Article>> SearchAsync(string keyword, int limit, int offset)
    {
        var now = DateTime.UtcNow;
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var articles = await context.Articles
                                    .Include(a => a.Author)
                                    .Include(a => a.ArticleTags)
                                    .ThenInclude(at => at.Tag)
                                    .Include(a => a.Likes)
                                    .Where(searchFilter)
                                    .OrderByDescending(a => a.CreatedAt)
                                    .Skip(offset)
                                    .Take(limit)
                                    .ToListAsync();

        return articles;
    }

    public async Task<int> CountByAuthorAsync(UserId authorId)
    {
        return await context.Articles
                            .Where(a => a.AuthorId == authorId && a.IsPublished)
                            .CountAsync();
    }

    /// <summary>
    ///     公開記事のフィルタ条件を生成します。
    /// </summary>
    /// <param name="now">現在時刻（UTC）。この時刻以前に公開された記事のみを返します。</param>
    /// <returns>公開記事のフィルタ条件</returns>
    static Expression<Func<Article, bool>> IsPubliclyVisible(DateTime now)
    {
        return a => a.IsPublished && a.PublishedAt.HasValue && a.PublishedAt.Value <= now;
    }

    /// <summary>
    ///     公開記事かつキーワードを含む記事のフィルタ条件を生成します。
    /// </summary>
    /// <param name="now">現在時刻（UTC）</param>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>フィルタ条件</returns>
    static Expression<Func<Article, bool>> IsPubliclyVisibleAndContainsKeyword(DateTime now, string keyword)
    {
        return a => a.IsPublished && a.PublishedAt.HasValue && a.PublishedAt.Value <= now &&
                    (a.Title.Contains(keyword) || a.Body.Contains(keyword));
    }
}
