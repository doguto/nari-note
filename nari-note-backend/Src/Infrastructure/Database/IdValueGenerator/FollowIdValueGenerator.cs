using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class FollowIdValueGenerator : ValueGenerator<FollowId>
{
    public override bool GeneratesTemporaryValues => false;

    public override FollowId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Follows;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return FollowId.From(next);

        static int MaxFrom(IEnumerable<Follow> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}