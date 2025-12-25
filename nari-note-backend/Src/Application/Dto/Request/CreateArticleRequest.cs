using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class CreateArticleRequest
{
    [Required(ErrorMessage = "タイトルは必須です")]
    [MaxLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "本文は必須です")]
    public string Body { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "著者IDは必須です")]
    [Range(1, int.MaxValue, ErrorMessage = "著者IDは1以上の値を指定してください")]
    public int AuthorId { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; } = false;
}
