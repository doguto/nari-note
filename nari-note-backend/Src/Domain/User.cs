using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Domain;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? ProfileImage { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public List<Article> Articles { get; set; } = new();
    public List<Session> Sessions { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public List<Follow> Followings { get; set; } = new();
    public List<Follow> Followers { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}
