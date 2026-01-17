using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class FollowerUserDto
{
    public UserId Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
}
