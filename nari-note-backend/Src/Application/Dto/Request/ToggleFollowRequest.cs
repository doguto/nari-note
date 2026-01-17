using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class ToggleFollowRequest
{
    public UserId FollowingId { get; set; }
}
