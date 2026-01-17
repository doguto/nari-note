using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class ToggleFollowRequest
{
    [Required(ErrorMessage = "フォロー対象のユーザーIDは必須です")]
    public UserId FollowingId { get; set; }
}
