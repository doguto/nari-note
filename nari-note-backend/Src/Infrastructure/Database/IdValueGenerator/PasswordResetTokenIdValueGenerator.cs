using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class PasswordResetTokenIdValueGenerator : ValueGenerator<PasswordResetTokenId>
{
    public override bool GeneratesTemporaryValues => false;

    public override PasswordResetTokenId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).PasswordResetTokens;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return PasswordResetTokenId.From(next);

        static int MaxFrom(IEnumerable<PasswordResetToken> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}
