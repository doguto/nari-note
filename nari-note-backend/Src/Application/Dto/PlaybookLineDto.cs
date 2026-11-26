using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookLineDto
{
    public PlaybookPositionId FromPositionId { get; set; }

    [Required]
    [MaxLength(300, ErrorMessage = "一度に保存できる手順は300手までです")]
    public List<string> Usis { get; set; } = new();
}
