using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class GetArticleRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Article ID must be a positive integer")]
    public int Id { get; set; }
}
