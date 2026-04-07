using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class CreateCommentResponse
{
    public CommentId Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
