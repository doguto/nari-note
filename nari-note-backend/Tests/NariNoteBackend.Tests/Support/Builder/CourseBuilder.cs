using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

/// <summary>
///     既定値で「公開済みの講座」を組み立てる。
/// </summary>
public class CourseBuilder
{
    readonly CourseId id = CourseId.From(Guid.CreateVersion7());
    readonly User owner;
    string name = "テスト講座";
    DateTime? publishedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1);

    public CourseBuilder(User owner)
    {
        this.owner = owner;
    }

    public CourseBuilder WithName(string value)
    {
        this.name = value;
        return this;
    }

    public CourseBuilder PublishedAt(DateTime value)
    {
        this.publishedAt = value;
        return this;
    }

    public CourseBuilder Draft()
    {
        this.publishedAt = null;
        return this;
    }

    public Course Build()
    {
        return new Course
        {
            Id = this.id,
            Name = this.name,
            UserId = this.owner.Id,
            User = this.owner,
            PublishedAt = this.publishedAt,
            CreatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1),
            UpdatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1)
        };
    }
}
