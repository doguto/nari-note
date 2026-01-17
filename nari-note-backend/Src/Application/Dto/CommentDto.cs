using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class CommentDto
{
    public CommentId Id { get; set; }
    public UserId UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
