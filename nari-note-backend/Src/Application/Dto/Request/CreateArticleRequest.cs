using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class CreateArticleRequest
{
    [Required(ErrorMessage = "タイトルは必須です")]
    [MaxLength(50, ErrorMessage = "タイトルは50文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "本文は必須です")]
    public string Body { get; set; } = string.Empty;
    
    public int AuthorId { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; } = false;
}
