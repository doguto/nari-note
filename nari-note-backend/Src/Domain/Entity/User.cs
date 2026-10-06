using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(Email), IsUnique = true)]
public class User : EntityBase
{
    [Key]
    public UserId Id { get; set; }

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

    public bool IsEmailVerified { get; set; } = false;

    // 名前の重複判定は、大文字小文字・前後空白を無視して行う
    public static string NormalizeName(string name)
    {
        return name.Trim().ToLowerInvariant();
    }

    // メールアドレスは小文字・前後空白除去で正規化して保存・照合する
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    // Navigation Properties
    public List<Article> Articles { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public List<CourseLike> CourseLikes { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Follow> Followings { get; set; } = new();  // 自分がフォローしているユーザーとの関係
    public List<Follow> Followers { get; set; } = new();   // 自分をフォローしているユーザーとの関係
    public List<Notification> Notifications { get; set; } = new();
    public List<EmailVerification> EmailVerifications { get; set; } = new();
}
