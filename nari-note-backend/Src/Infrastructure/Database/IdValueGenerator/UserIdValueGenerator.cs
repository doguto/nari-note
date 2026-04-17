using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class UserIdValueGenerator : ValueGenerator<UserId>
{
    public override bool GeneratesTemporaryValues => false;

    public override UserId Next(EntityEntry entry)
    {
        return UserId.From(Guid.CreateVersion7());
    }
}
