using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto;

public class KifuDto
{
    [Required]
    [MaxLength(100, ErrorMessage = "棋譜名は100文字以内で入力してください")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(20000, ErrorMessage = "棋譜テキストは20000文字以内で入力してください")]
    public required string KifuText { get; set; }

    public int SortOrder { get; set; }
}
