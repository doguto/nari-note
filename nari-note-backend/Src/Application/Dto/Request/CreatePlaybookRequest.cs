using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class CreatePlaybookRequest
{
    [Required(ErrorMessage = "定跡の名前は必須です")]
    [MaxLength(100, ErrorMessage = "定跡の名前は100文字以内で入力してください")]
    public required string Name { get; set; }

    // 省略すると平手の初期局面から始める
    [MaxLength(200, ErrorMessage = "SFENは200文字以内で入力してください")]
    public string? RootSfen { get; set; }
}
