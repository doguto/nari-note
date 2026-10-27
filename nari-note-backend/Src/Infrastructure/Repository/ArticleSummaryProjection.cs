using System.Linq.Expressions;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;

namespace NariNoteBackend.Infrastructure.Repository;

static class ArticleSummaryProjection
{
    /// <summary>
    ///     一覧のサムネイル表示に必要な列だけを射影します。
    ///     タグを JOIN するため、いいね数のサブクエリは記事 × タグの行ごとに実行されます。
    ///     AsSplitQuery でタグを分けると、ページングの絞り込みが 2 本目でも再実行され、深いページほど遅くなるため 1 本のクエリにしています。
    /// </summary>
    public static readonly Expression<Func<Article, ArticleSummary>> FromArticle = a => new ArticleSummary
    {
        Id = a.Id,
        Title = a.Title,
        AuthorId = a.AuthorId,
        AuthorName = a.Author.Name,
        AuthorProfileImage = a.Author.ProfileImage,
        TagNames = a.ArticleTags.OrderBy(at => at.Id).Select(at => at.Tag.Name).ToList(),
        LikeCount = a.Likes.Count(),
        PublishedAt = a.PublishedAt,
        UpdatedAt = a.UpdatedAt
    };
}
