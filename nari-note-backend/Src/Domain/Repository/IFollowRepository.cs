using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IFollowRepository : IRepository<Follow, FollowId>
{
    Task<Follow?> FindByFollowerAndFollowingAsync(UserId followerId, UserId followingId);
    Task<int> CountFollowersAsync(UserId userId);
    Task<int> CountFollowingsAsync(UserId userId);
    Task<(List<User> Users, int TotalCount)> GetFollowersAsync(UserId userId, int limit, int offset);
    Task<(List<User> Users, int TotalCount)> GetFollowingsAsync(UserId userId, int limit, int offset);
}
