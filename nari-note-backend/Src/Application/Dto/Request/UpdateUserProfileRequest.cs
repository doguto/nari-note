using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateUserProfileRequest
{
    [MaxLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    public string? Name { get; set; }
    
    [MaxLength(255, ErrorMessage = "プロフィール画像のURLは255文字以内で入力してください")]
    public string? ProfileImage { get; set; }
    
    [MaxLength(500, ErrorMessage = "自己紹介は500文字以内で入力してください")]
    public string? Bio { get; set; }
}
