using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetUserProfileResponse
{
    public UserId Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowing { get; set; }
    public int ArticleCount { get; set; }
    public int LikedArticleCount { get; set; }
    // 注意: PasswordHash と Email は含めない
}
