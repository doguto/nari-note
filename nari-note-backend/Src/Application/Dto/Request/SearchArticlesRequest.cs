using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SearchArticlesRequest
{
    [Required(ErrorMessage = "検索キーワードは必須です")]
    [MinLength(1, ErrorMessage = "検索キーワードは1文字以上で入力してください")]
    public string Keyword { get; set; } = string.Empty;
    
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
