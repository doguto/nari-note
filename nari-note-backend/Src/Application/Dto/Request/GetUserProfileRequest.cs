using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetUserProfileRequest
{
    public UserId Id { get; set; }
}
