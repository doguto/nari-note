using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

// 保存していない局面も解析できるよう、局面は SFEN で受け取る
public class AnalyzeBookPositionRequest
{
    public BookId Id { get; set; }

    [Required(ErrorMessage = "局面は必須です")]
    [MaxLength(200)]
    public required string Sfen { get; set; }
}
