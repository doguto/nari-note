using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetFollowersRequest
{
    public UserId UserId { get; set; }
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
