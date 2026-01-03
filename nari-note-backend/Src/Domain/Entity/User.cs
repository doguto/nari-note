using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(Email), IsUnique = true)]
public class User : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? ProfileImage { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    // Navigation Properties
    public List<Article> Articles { get; set; } = new();
    public List<Session> Sessions { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public List<Follow> Followings { get; set; } = new();  // 自分がフォローしているユーザーとの関係
    public List<Follow> Followers { get; set; } = new();   // 自分をフォローしているユーザーとの関係
    public List<Notification> Notifications { get; set; } = new();
}
