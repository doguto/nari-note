using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateShapeGroupRequest
{
    public PlaybookId Id { get; set; }
    public ShapeGroupId GroupId { get; set; }

    [MaxLength(100, ErrorMessage = "形の名前は100文字以内で入力してください")]
    public string? Name { get; set; }

    [MaxLength(10000, ErrorMessage = "研究メモは10000文字以内で入力してください")]
    public string? Memo { get; set; }
}
