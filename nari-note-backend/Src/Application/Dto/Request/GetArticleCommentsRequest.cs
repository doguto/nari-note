using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetArticleCommentsRequest
{
    [Required]
    public ArticleId Id { get; set; }
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
