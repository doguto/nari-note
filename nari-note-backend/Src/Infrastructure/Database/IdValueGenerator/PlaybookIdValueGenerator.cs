using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class PlaybookIdValueGenerator : ValueGenerator<PlaybookId>
{
    public override bool GeneratesTemporaryValues => false;

    public override PlaybookId Next(EntityEntry entry)
    {
        return PlaybookId.From(Guid.CreateVersion7());
    }
}
