using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateCourseRequest
{
    public CourseId Id { get; set; }
    
    [MaxLength(100)]
    public string? Name { get; set; }
    
    public bool? IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
