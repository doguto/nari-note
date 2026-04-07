using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetFollowersRequest
{
    public UserId UserId { get; set; }
}
