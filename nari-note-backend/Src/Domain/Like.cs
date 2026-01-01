using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(UserId), nameof(ArticleId), IsUnique = true)]
public class Like : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Article")]
    public int ArticleId { get; set; }


    // Navigation Properties
    public required User User { get; set; }
    public required Article Article { get; set; }
}
