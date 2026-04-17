using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class CourseIdValueGenerator : ValueGenerator<CourseId>
{
    public override bool GeneratesTemporaryValues => false;

    public override CourseId Next(EntityEntry entry)
    {
        return CourseId.From(Guid.CreateVersion7());
    }
}
