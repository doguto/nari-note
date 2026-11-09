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

        // 講座の記事はタグ別の講座一覧側で講座として表示するため除外する
        var query = context.Articles
                           .Where(a => a.ArticleTags.Any(at => tagIds.Contains(at.TagId)))
                           .Where(IsStandalone())
                           .Where(visibilityFilter);

        var totalCount = await query.CountAsync();
        var articles = await query.OrderByDescending(a => a.PublishedAt)
                                  .ThenByDescending(a => a.Id)
                                  .Skip(offset)
                                  .Take(limit)
                                  .Select(ArticleSummaryProjection.FromArticle)
                                  .ToListAsync();

        return (articles, totalCount);
    }

    public async Task<(List<ArticleSummary> Articles, int TotalCount)> FindPublishedSummariesByAuthorAsync(UserId authorId, int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var visibilityFilter = IsPubliclyVisible(now);

        // 下書きに加え、公開日時を過ぎていない予約投稿も第三者には見せない
        // 講座に属する記事は講座一覧側で表示するため除外する
        var query = context.Articles
                           .Where(a => a.AuthorId == authorId)
                           .Where(IsStandalone())
                           .Where(visibilityFilter);

        var totalCount = await query.CountAsync();
        var articles = await query.OrderByDescending(a => a.PublishedAt)
                                  .ThenByDescending(a => a.Id)
                                  .Skip(offset)
                                  .Take(limit)
                                  .Select(ArticleSummaryProjection.FromArticle)
                                  .ToListAsync();

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
                           .Where(IsStandalone())
                           .OrderByDescending(a => a.PublishedAt)
                           .ThenByDescending(a => a.Id)
                           .Skip(offset)
                           .Take(limit);

        return await query.Select(ArticleSummaryProjection.FromArticle).ToListAsync();
    }

    public async Task<List<ArticleSummary>> FindDraftSummariesByAuthorAsync(UserId authorId)
    {
        var query = context.Articles
                           .Where(a => a.AuthorId == authorId && !a.PublishedAt.HasValue)
                           .OrderByDescending(a => a.UpdatedAt)
                           .ThenByDescending(a => a.Id);

        return await query.Select(ArticleSummaryProjection.FromArticle).ToListAsync();
    }

    public async Task<(List<ArticleSummary> Articles, int TotalCount)> SearchSummariesAsync(string keyword, int limit, int offset)
    {
        var now = timeProvider.UtcNow();
        var searchFilter = IsPubliclyVisibleAndContainsKeyword(now, keyword);

        var query = context.Articles
                           .Where(IsStandalone())
                           .Where(searchFilter);

        var totalCount = await query.CountAsync();
        var articles = await query.OrderByDescending(a => a.CreatedAt)
                                  .ThenByDescending(a => a.Id)
                                  .Skip(offset)
                                  .Take(limit)
                                  .Select(ArticleSummaryProjection.FromArticle)
                                  .ToListAsync();

        return (articles, totalCount);
    }

    public async Task<int> CountByAuthorAsync(UserId authorId)
    {
        var now = timeProvider.UtcNow();
        var visibilityFilter = IsPubliclyVisible(now);

        // 作者別の記事一覧と件数を揃えるため、講座に属する記事は数えない
        return await context.Articles
                            .Where(a => a.AuthorId == authorId)
                            .Where(IsStandalone())
                            .Where(visibilityFilter)
                            .CountAsync();
    }

    // Likes 行をロードせず、サブクエリ COUNT でいいね数を取得して Article.LikeCount に設定します。
    static async Task<List<Article>> ToListWithLikeCountAsync(IQueryable<Article> query)
    {
        var rows = await query.Select(a => new { Article = a, LikeCount = a.Likes.Count() })
                              .ToListAsync();

        foreach (var row in rows) row.Article.LikeCount = row.LikeCount;

        return rows.Select(row => row.Article).ToList();
    }

    static Expression<Func<Article, bool>> IsPubliclyVisible(DateTime now)
    {
        // a.IsPubliclyVisible は DB に登録されたカラムではなく Property なため、Where 句では PublishedAt で判定する
        return a => a.PublishedAt.HasValue && a.PublishedAt!.Value <= now;
    }

    // 講座に属さない単発記事のフィルタ条件
    // 新着・検索・作者別・タグ別のように講座側にも同種の一覧がある一覧では、講座の記事を除外するために使う
    // いいね一覧のように講座側に対応する一覧が無いものでは使わない
    static Expression<Func<Article, bool>> IsStandalone()
    {
        return a => !a.CourseId.HasValue;
    }

    // 公開記事かつキーワードを含む記事のフィルタ条件を生成します。
    // 講座に属する記事は講座検索側で講座としてヒットさせるため、呼び出し側で IsStandalone と組み合わせます。
    static Expression<Func<Article, bool>> IsPubliclyVisibleAndContainsKeyword(DateTime now, string keyword)
    {
        return a => a.PublishedAt.HasValue && a.PublishedAt.Value <= now &&
                    (a.Title.Contains(keyword) || a.Body.Contains(keyword));
    }
}
