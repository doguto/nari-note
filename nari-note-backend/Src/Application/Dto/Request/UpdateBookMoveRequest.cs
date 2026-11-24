using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateBookMoveRequest
{
    public BookId Id { get; set; }
    public BookMoveId MoveId { get; set; }

    [MaxLength(200, ErrorMessage = "メモは200文字以内で入力してください")]
    public string? Memo { get; set; }

    [MaxLength(100, ErrorMessage = "条件は100文字以内で入力してください")]
    public string? Condition { get; set; }
}
