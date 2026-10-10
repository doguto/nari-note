using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

/// <summary>
///     既定値で「公開済みの単体記事」を組み立てる。
/// </summary>
public class ArticleBuilder
{
    readonly User author;
    readonly ArticleId id = ArticleId.From(Guid.CreateVersion7());
    int? articleOrder;
    string body = "テスト本文";
    Course? course;
    DateTime createdAt = TestTimeProvider.DefaultUtcNow.AddDays(-1);
    DateTime? publishedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1);
    string title = "テスト記事";

    public ArticleBuilder(User author)
    {
        this.author = author;
    }

    public ArticleBuilder WithTitle(string value)
    {
        this.title = value;
        return this;
    }

    public ArticleBuilder WithBody(string value)
    {
        this.body = value;
        return this;
    }

    public ArticleBuilder PublishedAt(DateTime value)
    {
        this.publishedAt = value;
        return this;
    }

    public ArticleBuilder Draft()
    {
        this.publishedAt = null;
        return this;
    }

    public ArticleBuilder CreatedAt(DateTime value)
    {
        this.createdAt = value;
        return this;
    }

    public ArticleBuilder InCourse(Course value, int? order = null)
    {
        this.course = value;
        this.articleOrder = order;
        return this;
    }

    public Article Build()
    {
        return new Article
        {
            Id = this.id,
            Title = this.title,
            Body = this.body,
            AuthorId = this.author.Id,
            Author = this.author,
            CourseId = this.course?.Id,
            Course = this.course,
            ArticleOrder = this.articleOrder,
            PublishedAt = this.publishedAt,
            CreatedAt = this.createdAt,
            UpdatedAt = this.createdAt
        };
    }
}
