using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class TagIdValueGenerator : ValueGenerator<TagId>
{
    public override bool GeneratesTemporaryValues => false;

    public override TagId Next(EntityEntry entry)
    {
        return TagId.From(Guid.CreateVersion7());
    }
}
