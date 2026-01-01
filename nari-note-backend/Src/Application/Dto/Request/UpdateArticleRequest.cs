using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateArticleRequest
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string? Title { get; set; }
    
    [MaxLength(10000)]
    public string? Body { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPublished { get; set; }
}
