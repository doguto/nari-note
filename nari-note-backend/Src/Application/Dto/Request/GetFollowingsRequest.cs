using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetFollowingsRequest
{
    public UserId UserId { get; set; }
}
