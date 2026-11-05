using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(Token), IsUnique = true)]
public class EmailVerification : EntityBase
{
    [Key]
    public EmailVerificationId Id { get; set; }

    public required UserId UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    // サインアップ時に入力されたユーザー名・パスワード。このトークンで認証したときにユーザーへ反映する
    // 確認メールを受け取った本人が入力した値で認証させ、他人が先に登録したパスワードが残らないようにするため
    // 確認メールの再送で発行したトークンは null（ユーザーの値をそのまま使う）
    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
