using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class CreateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
