using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class SavePlaybookLinesRequest
{
    public PlaybookId Id { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "保存する手順がありません")]
    [MaxLength(100, ErrorMessage = "一度に保存できる手順は100本までです")]
    public List<PlaybookLineDto> Lines { get; set; } = new();
}
