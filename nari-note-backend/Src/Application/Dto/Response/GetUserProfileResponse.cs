namespace NariNoteBackend.Application.Dto.Response;

public class GetUserProfileResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    // 注意: PasswordHash と Email は含めない
}
