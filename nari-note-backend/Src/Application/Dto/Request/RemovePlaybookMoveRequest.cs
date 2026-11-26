using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class RemovePlaybookMoveRequest
{
    public PlaybookId Id { get; set; }

    [Required(ErrorMessage = "局面は必須です")]
    [MaxLength(PlaybookBoard.SfenMaxLength)]
    public required string Sfen { get; set; }

    [Required(ErrorMessage = "指し手は必須です")]
    [MaxLength(PlaybookBoardTransition.UsiMaxLength)]
    public required string Usi { get; set; }
}
