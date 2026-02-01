using System.ComponentModel.DataAnnotations;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetArticleContentRequest
{
    [Required]
    public ArticleId Id { get; set; }
}
