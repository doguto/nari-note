using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class ArticleIdValueGenerator : ValueGenerator<ArticleId>
{
    public override bool GeneratesTemporaryValues => false;

    public override ArticleId Next(EntityEntry entry)
    {
        return ArticleId.From(Guid.CreateVersion7());
    }
}
