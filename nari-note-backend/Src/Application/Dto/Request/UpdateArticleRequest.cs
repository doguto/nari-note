using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateArticleRequest
{
    public int Id { get; set; }
    public int UserId { get; set; } // 認証実装後は削除
    
    [MaxLength(200)]
    public string? Title { get; set; }
    public string? Body { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPublished { get; set; }
}
