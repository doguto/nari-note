using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.ReadModel;

// 記事一覧のサムネイル表示用。本文などの重い列を読み込まないよう、必要な列だけを射影して取得する
public class ArticleSummary
{
    public ArticleId Id { get; init; }
    public required string Title { get; init; }
    public UserId AuthorId { get; init; }
    public required string AuthorName { get; init; }
    public string? AuthorProfileImage { get; init; }
    public List<string> TagNames { get; init; } = new();
    public int LikeCount { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public bool IsPublished => PublishedAt.HasValue;
}
