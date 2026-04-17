using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class EmailVerificationIdValueGenerator : ValueGenerator<EmailVerificationId>
{
    public override bool GeneratesTemporaryValues => false;

    public override EmailVerificationId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).EmailVerifications;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return EmailVerificationId.From(next);

        static int MaxFrom(IEnumerable<EmailVerification> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}
