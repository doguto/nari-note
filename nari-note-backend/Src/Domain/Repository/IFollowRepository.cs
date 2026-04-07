using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IFollowRepository : IRepository<Follow, FollowId>
{
    Task<Follow?> FindByFollowerAndFollowingAsync(UserId followerId, UserId followingId);
    Task<int> CountFollowersAsync(UserId userId);
    Task<int> CountFollowingsAsync(UserId userId);
    Task<List<User>> GetFollowersAsync(UserId userId);
    Task<List<User>> GetFollowingsAsync(UserId userId);
}
