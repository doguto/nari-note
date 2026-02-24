using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class FollowerUserDto
{
    public UserId Id { get; set; }
    public required string Username { get; set; }
    public string? ProfileImage { get; set; }
}
