using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

/// <summary>
///     既定値で有効な User を組み立てる。テストに関係する項目だけ With* で上書きする。
/// </summary>
public class UserBuilder
{
    // Name / Email は一意制約があるため連番で重複を避ける
    static int sequence;

    readonly UserId id = UserId.From(Guid.CreateVersion7());
    string? bio;
    string email;
    bool isEmailVerified = true;
    string name;
    string passwordHash = "test-password-hash";
    string? profileImage;

    public UserBuilder()
    {
        var number = Interlocked.Increment(ref sequence);
        this.name = $"user{number}";
        this.email = $"user{number}@example.com";
    }

    public UserBuilder WithName(string value)
    {
        this.name = value;
        return this;
    }

    public UserBuilder WithEmail(string value)
    {
        this.email = value;
        return this;
    }

    // サインインを通すテスト用。計算を軽くするため最小のワークファクターでハッシュ化する
    public UserBuilder WithPassword(string plainPassword)
    {
        this.passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, 4);
        return this;
    }

    public UserBuilder WithBio(string value)
    {
        this.bio = value;
        return this;
    }

    public UserBuilder WithProfileImage(string value)
    {
        this.profileImage = value;
        return this;
    }

    public UserBuilder EmailUnverified()
    {
        this.isEmailVerified = false;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = this.id,
            Name = this.name,
            Email = this.email,
            PasswordHash = this.passwordHash,
            Bio = this.bio,
            ProfileImage = this.profileImage,
            IsEmailVerified = this.isEmailVerified,
            CreatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-30),
            UpdatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-30)
        };
    }
}
