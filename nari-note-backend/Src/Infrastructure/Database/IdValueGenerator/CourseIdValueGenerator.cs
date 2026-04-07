using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class CourseIdValueGenerator : ValueGenerator<CourseId>
{
    public override bool GeneratesTemporaryValues => false;

    public override CourseId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Courses;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return CourseId.From(next);

        static int MaxFrom(IEnumerable<Course> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}