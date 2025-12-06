using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Domain;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public List<Article> Articles { get; set; } = new();
}
