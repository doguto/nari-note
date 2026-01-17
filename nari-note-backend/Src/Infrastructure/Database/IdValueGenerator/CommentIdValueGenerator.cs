using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class CommentIdValueGenerator : ValueGenerator<CommentId>
{
    public override bool GeneratesTemporaryValues => false;

    public override CommentId Next(EntityEntry entry)
    {
        var entities = ((NariNoteDbContext)entry.Context).Comments;
        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;
        return CommentId.From(next);

        static int MaxFrom(IEnumerable<Comment> entities)
        {
            return entities.Any() ? entities.Max(e => e.Id.Value) : 0;
        }
    }
}