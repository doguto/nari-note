using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    readonly NariNoteDbContext context;
    readonly TimeProvider timeProvider;

    public ArticleRepository(NariNoteDbContext context, TimeProvider timeProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public async Task<Article> CreateAsync(Article article)
    {
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }

    public async Task<Article?> FindByIdAsync(ArticleId id)
    {
        var query = context.Articles
                            .Include(a => a.Author)
                            .Include(a => a.Course)
                            .Include(a => a.ArticleTags)
                            .ThenInclude(at => at.Tag)
                            .Include(a => a.Kifus)
                            .AsSplitQuery()
                            .Where(a => a.Id == id);

        return (await ToListWithLikeCountAsync(query)).FirstOrDefault();
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
        var query = context.Articles
                           .AsNoTracking()
                           .Include(a => a.Author)
                           .Include(a => a.ArticleTags)
                           .ThenInclude(at => at.Tag)
                           .AsSplitQuery()
                           .Where(a => a.AuthorId == authorId)
                           .OrderByDescending(a => a.PublishedAt)
                           .ThenByDescending(a => a.Id);

        return await ToListWithLikeCountAsync(query);
    }

    public async Task<(List<ArticleSummary> Articles, int TotalCount)> FindSummariesByTagAsync(string tagName, int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var visibilityFilter = IsPubliclyVisible(now);

        // タグ ID を先に確定させ、プランナが ArticleTags.TagId の統計から件数を見積もれるようにする
        // タグ名は大文字小文字を区別せずに一致させる（大文字小文字違いの別タグも含む）
        var normalizedTagName = tagName.ToLowerInvariant();
        var tagIds = await context.Tags
                                  .Where(t => t.Name.ToLower() == normalizedTagName)
                                  .Select(t => t.Id)
                                  .ToListAsync();
        if (tagIds.Count == 0) return (new List<ArticleSummary>(), 0);

        var query = context.Articles
                           .Where(a => a.ArticleTags.Any(at => tagIds.Contains(at.TagId)))
                           .Where(visibilityFilter);

        var totalCount = await query.CountAsync();
        var articles = await ToSummaryListAsync(query.OrderByDescending(a => a.PublishedAt)
                                                     .ThenByDescending(a => a.Id)
                                                     .Skip(offset)
                                                     .Take(limit));

        return (articles, totalCount);
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
                    CreatedAt = timeProvider.UtcNow(),
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

    public async Task<List<ArticleSummary>> FindLatestSingleSummariesAsync(int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var visibilityFilter = IsPubliclyVisible(now);

        // 講座の記事は取得しない
        var query = context.Articles
                           .Where(visibilityFilter)
                           .Where(a => !a.CourseId.HasValue)
                           .OrderByDescending(a => a.PublishedAt)
                           .ThenByDescending(a => a.Id)
                           .Skip(offset)
                           .Take(limit);

        return await ToSummaryListAsync(query);
    }

    public async Task<List<ArticleSummary>> FindDraftSummariesByAuthorAsync(UserId authorId)
    {
        var query = context.Articles
                           .Where(a => a.AuthorId == authorId && !a.PublishedAt.HasValue)
                           .OrderByDescending(a => a.UpdatedAt)
                           .ThenByDescending(a => a.Id);

        return await ToSummaryListAsync(query);
    }

    public async Task<List<Article>> SearchAsync(string keyword, int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var articles = await context.Articles
                                    .Include(a => a.Author)
                                    .Include(a => a.ArticleTags)
                                    .ThenInclude(at => at.Tag)
                                            .Where(searchFilter)
                                    .OrderByDescending(a => a.CreatedAt)
                                    .Skip(offset)
                                    .Take(limit)
                                    .ToListAsync();

        return articles;
    }

    public async Task<int> CountByAuthorAsync(UserId authorId)
    {
        var now = timeProvider.UtcNow();
        var visibilityFilter = IsPubliclyVisible(now);

        return await context.Articles
                            .Where(a => a.AuthorId == authorId)
                            .Where(visibilityFilter)
                            .CountAsync();
    }

    /// <summary>
    ///     Likes 行をロードせず、サブクエリ COUNT でいいね数を取得して Article.LikeCount に設定します。
    /// </summary>
    static async Task<List<Article>> ToListWithLikeCountAsync(IQueryable<Article> query)
    {
        var rows = await query.Select(a => new { Article = a, LikeCount = a.Likes.Count() })
                              .ToListAsync();

        foreach (var row in rows) row.Article.LikeCount = row.LikeCount;

        return rows.Select(row => row.Article).ToList();
    }

    /// <summary>
    ///     一覧のサムネイル表示に必要な列だけを射影して取得します。
    ///     タグを JOIN するため、いいね数のサブクエリは記事 × タグの行ごとに実行されます。
    ///     AsSplitQuery でタグを分けると、ページングの絞り込みが 2 本目でも再実行され、深いページほど遅くなるため 1 本のクエリにしています。
    /// </summary>
    static async Task<List<ArticleSummary>> ToSummaryListAsync(IQueryable<Article> query)
    {
        return await query.Select(a => new ArticleSummary
                          {
                              Id = a.Id,
                              Title = a.Title,
                              AuthorId = a.AuthorId,
                              AuthorName = a.Author.Name,
                              AuthorProfileImage = a.Author.ProfileImage,
                              TagNames = a.ArticleTags.OrderBy(at => at.Id).Select(at => at.Tag.Name).ToList(),
                              LikeCount = a.Likes.Count(),
                              PublishedAt = a.PublishedAt,
                              UpdatedAt = a.UpdatedAt
                          })
                          .ToListAsync();
    }

    static Expression<Func<Article, bool>> IsPubliclyVisible(DateTime now)
    {
        // a.IsPubliclyVisible は DB に登録されたカラムではなく Property なため、Where 句では PublishedAt で判定する
        return a => a.PublishedAt.HasValue && a.PublishedAt!.Value <= now;
    }

    /// <summary>
    ///     公開記事かつキーワードを含む記事のフィルタ条件を生成します。
    /// </summary>
    /// <param name="now">現在時刻（UTC）</param>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>フィルタ条件</returns>
    static Expression<Func<Article, bool>> IsPubliclyVisibleAndContainsKeyword(DateTime now, string keyword)
    {
        return a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now &&
                    (a.Title.Contains(keyword) || a.Body.Contains(keyword));
    }
}
