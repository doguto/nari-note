using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto;

// 開始局面からの手順
public class PlaybookLineDto
{
    [Required]
    [MinLength(1, ErrorMessage = "保存する手順がありません")]
    [MaxLength(300, ErrorMessage = "一度に保存できる手順は300手までです")]
    public List<string> Usis { get; set; } = new();
}
