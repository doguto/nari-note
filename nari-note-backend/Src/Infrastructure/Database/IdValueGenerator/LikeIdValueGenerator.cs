using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class LikeIdValueGenerator : ValueGenerator<LikeId>
{
    public override bool GeneratesTemporaryValues => false;

    public override LikeId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Likes;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return LikeId.From(next);

        static int MaxFrom(IEnumerable<Like> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}