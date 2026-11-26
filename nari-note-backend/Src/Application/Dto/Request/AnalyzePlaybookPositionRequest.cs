using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

// 保存していない局面も解析できるよう、局面は SFEN で受け取る
public class AnalyzePlaybookPositionRequest
{
    public PlaybookId Id { get; set; }

    [Required(ErrorMessage = "局面は必須です")]
    [MaxLength(200)]
    public required string Sfen { get; set; }

    [Required(ErrorMessage = "エンジンは必須です")]
    [AllowedValues(AnalysisEngine.TsuyouraouMini, AnalysisEngine.TsuyouraouMicro, ErrorMessage = "未対応のエンジンです")]
    public required string Engine { get; set; }
}
