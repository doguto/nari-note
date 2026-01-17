using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class SearchArticlesRequest
{
    [MinLength(1, ErrorMessage = "検索キーワードは1文字以上で入力してください")]
    public string Keyword { get; set; } = string.Empty;
    
    [Range(1, 100, ErrorMessage = "取得件数は1から100の範囲で指定してください")]
    public int Limit { get; set; } = 20;
    
    [Range(0, int.MaxValue, ErrorMessage = "オフセットは0以上の値を指定してください")]
    public int Offset { get; set; } = 0;
}
