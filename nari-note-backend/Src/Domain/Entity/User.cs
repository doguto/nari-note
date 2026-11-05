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

    // 発行済みの認証トークン（JWT）を一括で無効にするための値。JWT に同じ値を埋め込み、一致しないトークンは無効として扱う
    // null の間は照合しない（導入前に発行されたトークンを無効にしないため）
    [MaxLength(64)]
    public string? SecurityStamp { get; set; }

    // 登録済みのメールアドレスでサインアップされたときに、その旨の案内メールを最後に送った日時（連続送信の防止に使う）
    public DateTime? AlreadyRegisteredNotifiedAt { get; set; }

    // 発行済みの認証トークンをすべて無効にする
    public void RotateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString("N");
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
