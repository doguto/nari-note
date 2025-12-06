using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain;

public class Follow
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Following")]
    public int FollowingId { get; set; }

    [Required]
    [ForeignKey("Followed")]
    public int FollowedId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public required User Following { get; set; }
    public required User Followed { get; set; }
}
