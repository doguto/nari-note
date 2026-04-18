using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class KifuIdValueGenerator : ValueGenerator<KifuId>
{
    public override bool GeneratesTemporaryValues => false;

    public override KifuId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Kifus;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return KifuId.From(next);

        static int MaxFrom(IEnumerable<Kifu> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}