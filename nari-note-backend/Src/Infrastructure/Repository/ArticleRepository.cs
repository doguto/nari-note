using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

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
        try
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
            return article;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("Article with this title already exists", ex);
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new ValidationException("Invalid reference to related entity", null, ex);
            }
            throw new InfrastructureException("Database error occurred while creating article", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified by another user", ex);
        }
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article> GetByIdAsync(int id)
    {
        var article = await FindByIdAsync(id);
        if (article == null) throw new NotFoundException($"記事{id}が存在しません");

        return article;
    }

    public async Task<List<Article>> FindByAuthorAsync(int authorId)
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
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .Where(a => a.ArticleTags.Any(at => EF.Functions.ILike(at.Tag.Name, tagName)))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Article> UpdateAsync(Article article, List<string>? tagNames = null)
    {
        try
        {
            context.Articles.Update(article);
            
            // Update tags if provided
            if (tagNames != null)
            {
                // Remove existing ArticleTags
                var existingArticleTags = await context.ArticleTags
                    .Where(at => at.ArticleId == article.Id)
                    .ToListAsync();
                context.ArticleTags.RemoveRange(existingArticleTags);
                
                if (tagNames.Count > 0)
                {
                    // Get existing tags
                    var existingTags = await context.Tags
                        .Where(t => tagNames.Contains(t.Name))
                        .ToListAsync();
                    
                    var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();
                    var newTagNames = tagNames.Where(tn => !existingTagNames.Contains(tn)).ToList();
                    
                    // Create new tags
                    var newTags = newTagNames.Select(name => new Tag
                    {
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                    
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
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified by another user", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("Duplicate tag association", ex);
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new ValidationException("Invalid reference to related entity", null, ex);
            }
            throw new InfrastructureException("Database error occurred while updating article", ex);
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var article = await context.Articles.FindAsync(id);
            if (article != null)
            {
                context.Articles.Remove(article);
                await context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified or deleted by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException($"Database error occurred while deleting article with ID {id}", ex);
        }
    }
}
