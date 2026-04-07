using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class ArticleTagIdValueGenerator : ValueGenerator<ArticleTagId>
{
    public override bool GeneratesTemporaryValues => false;

    public override ArticleTagId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).ArticleTags;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return ArticleTagId.From(next);

        static int MaxFrom(IEnumerable<ArticleTag> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}