using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

/// <summary>
///     既定値で「公開済みの講座」を組み立てる。
/// </summary>
public class CourseBuilder
{
    readonly List<Article> articles = new();
    readonly CourseId id = CourseId.From(Guid.CreateVersion7());
    readonly List<User> likers = new();
    DateTime createdAt = TestTimeProvider.DefaultUtcNow.AddDays(-1);
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

    public CourseBuilder CreatedAt(DateTime value)
    {
        this.createdAt = value;
        return this;
    }

    public CourseBuilder Draft()
    {
        this.publishedAt = null;
        return this;
    }

    /// <summary>単体テスト用に、読み込み済みの記事として設定する</summary>
    public CourseBuilder WithArticles(params Article[] values)
    {
        this.articles.AddRange(values);
        return this;
    }

    public CourseBuilder LikedBy(params User[] users)
    {
        this.likers.AddRange(users);
        return this;
    }

    public Course Build()
    {
        var course = new Course
        {
            Id = this.id,
            Name = this.name,
            UserId = this.owner.Id,
            User = this.owner,
            PublishedAt = this.publishedAt,
            CreatedAt = this.createdAt,
            UpdatedAt = this.createdAt
        };

        course.Articles.AddRange(this.articles);
        foreach (var liker in this.likers)
        {
            course.CourseLikes.Add(TestEntity.CourseLike(liker, course));
        }

        return course;
    }
}
