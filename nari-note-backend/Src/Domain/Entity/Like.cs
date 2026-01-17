using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(UserId), nameof(ArticleId), IsUnique = true)]
public class Like : EntityBase
{
    [Key]
    public LikeId Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public UserId UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public ArticleId ArticleId { get; set; }


    // Navigation Properties
    public User User { get; set; }
    public Article Article { get; set; }
}
