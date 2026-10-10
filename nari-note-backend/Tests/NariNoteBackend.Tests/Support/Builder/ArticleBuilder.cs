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
    readonly List<User> likers = new();
    readonly List<string> tagNames = new();
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

    /// <summary>タグを新規作成して紐づける（結合テストで既存のタグを使う場合は TestEntity.ArticleTag を使用する）</summary>
    public ArticleBuilder WithTags(params string[] names)
    {
        this.tagNames.AddRange(names);
        return this;
    }

    public ArticleBuilder LikedBy(params User[] users)
    {
        this.likers.AddRange(users);
        return this;
    }

    public Article Build()
    {
        var article = new Article
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

        foreach (var name in this.tagNames)
        {
            article.ArticleTags.Add(TestEntity.ArticleTag(article, TestEntity.Tag(name)));
        }

        foreach (var liker in this.likers)
        {
            article.Likes.Add(TestEntity.Like(liker, article));
        }

        return article;
    }
}
