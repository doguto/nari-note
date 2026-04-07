using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetCoursesByAuthorRequest
{
    public UserId AuthorId { get; set; }
}
