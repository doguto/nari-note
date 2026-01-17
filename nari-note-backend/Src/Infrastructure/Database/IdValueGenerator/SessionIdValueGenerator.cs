using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class SessionIdValueGenerator : ValueGenerator<SessionId>
{
    public override bool GeneratesTemporaryValues => false;

    public override SessionId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Sessions;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return SessionId.From(next);

        static int MaxFrom(IEnumerable<Session> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}