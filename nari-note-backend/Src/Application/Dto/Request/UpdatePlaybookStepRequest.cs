using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdatePlaybookStepRequest
{
    public PlaybookId Id { get; set; }
    public PlaybookNodeStepId StepId { get; set; }

    [Required]
    [MaxLength(PlaybookNodeStep.ConditionMaxLength, ErrorMessage = "条件は100文字以内で入力してください")]
    public required string Condition { get; set; }
}
