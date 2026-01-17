using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class ArticleIdValueGenerator : ValueGenerator<ArticleId>
{
    public override bool GeneratesTemporaryValues => false;

    public override ArticleId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Articles;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return ArticleId.From(next);

        static int MaxFrom(IEnumerable<Article> articles)
        {
            return articles.Any() ? articles.Max(e => e.Id.Value) : 0;
        }
    }
}
