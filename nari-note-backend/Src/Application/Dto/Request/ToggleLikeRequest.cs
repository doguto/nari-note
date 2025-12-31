using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class ToggleLikeRequest
{
    public int ArticleId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; } // 認証実装後は削除
}
