using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.ReadModel;

// 講座一覧の表示用。記事の本文などを読み込まないよう、必要な列だけを射影して取得する
public class CourseSummary
{
    public CourseId Id { get; init; }
    public UserId UserId { get; init; }
    public required string UserName { get; init; }
    public string? UserProfileImage { get; init; }
    public required string Name { get; init; }
    public List<CourseArticleSummary> Articles { get; init; } = new();
    public int LikeCount { get; init; }
    public DateTime? PublishedAt { get; init; }

    public bool IsPublished => PublishedAt.HasValue;
}

public class CourseArticleSummary
{
    public ArticleId Id { get; init; }
    public required string Title { get; init; }
}
