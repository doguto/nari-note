using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateArticleRequest
{
    public ArticleId Id { get; set; }
    
    [MaxLength(50)]
    public string? Title { get; set; }
    
    [MaxLength(10000)]
    public string? Body { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public CourseId? CourseId { get; set; }
    public int? ArticleOrder { get; set; }
}
