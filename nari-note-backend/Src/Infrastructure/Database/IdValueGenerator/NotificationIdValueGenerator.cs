using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class NotificationIdValueGenerator : ValueGenerator<NotificationId>
{
    public override bool GeneratesTemporaryValues => false;

    public override NotificationId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Notifications;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return NotificationId.From(next);

        static int MaxFrom(IEnumerable<Notification> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}