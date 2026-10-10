using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

/// <summary>
///     設定項目が少なく Builder を用意するほどでもない Entity を組み立てる。
///     ID は保存時に採番されるため、結合テストのデータ投入に使用する。
/// </summary>
public static class TestEntity
{
    public static Tag Tag(string name)
    {
        return new Tag { Id = TagId.From(Guid.CreateVersion7()), Name = name };
    }

    public static ArticleTag ArticleTag(Article article, Tag tag)
    {
        return new ArticleTag { ArticleId = article.Id, Article = article, TagId = tag.Id, Tag = tag };
    }

    public static Like Like(User user, Article article, DateTime? createdAt = null)
    {
        var like = new Like { UserId = user.Id, ArticleId = article.Id };
        if (createdAt.HasValue) like.CreatedAt = createdAt.Value;
        return like;
    }

    public static CourseLike CourseLike(User user, Course course)
    {
        return new CourseLike { UserId = user.Id, CourseId = course.Id };
    }

    public static Follow Follow(User follower, User following)
    {
        return new Follow { FollowerId = follower.Id, FollowingId = following.Id };
    }

    public static Comment Comment(User user, Article article, string message, DateTime? createdAt = null)
    {
        var comment = new Comment { UserId = user.Id, ArticleId = article.Id, Message = message };
        if (createdAt.HasValue) comment.CreatedAt = createdAt.Value;
        return comment;
    }

    public static Kifu Kifu(Article article, string name, int sortOrder = 0, string kifuText = "▲7六歩")
    {
        return new Kifu { ArticleId = article.Id, Name = name, KifuText = kifuText, SortOrder = sortOrder };
    }

    public static EmailVerification EmailVerification(User user, string token, DateTime expiresAt, bool isUsed = false)
    {
        return new EmailVerification { UserId = user.Id, Token = token, ExpiresAt = expiresAt, IsUsed = isUsed };
    }

    public static PasswordResetToken PasswordResetToken(
        User user,
        string token,
        DateTime expiresAt,
        bool isUsed = false
    )
    {
        return new PasswordResetToken { UserId = user.Id, Token = token, ExpiresAt = expiresAt, IsUsed = isUsed };
    }
}
