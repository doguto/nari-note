namespace NariNoteBackend.Domain.Entity;

public abstract class EntityBase
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
