using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class CommentDto
{
    public CommentId Id { get; set; }
    public UserId UserId { get; set; }
    public required string UserName { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
