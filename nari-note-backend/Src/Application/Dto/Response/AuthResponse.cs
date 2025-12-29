namespace NariNoteBackend.Application.Dto.Response;

public class AuthResponse
{
    public required string Token { get; set; }
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public DateTime ExpiresAt { get; set; }
}
