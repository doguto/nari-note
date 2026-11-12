# ER 図

主要カラムのみを示す。スキーマの正は `Migrations/NariNoteDbContextModelSnapshot.cs`（Entity は `Src/Domain/Entity/`、リレーション設定は `Src/Infrastructure/Database/NariNoteDbContext.cs`）。

```mermaid
erDiagram
    Users ||--o{ Articles : "書く"
    Users ||--o{ Courses : "作る"
    Courses |o--o{ Articles : "含む"
    Articles ||--o{ ArticleTags : ""
    Tags ||--o{ ArticleTags : ""
    Articles ||--o{ Kifus : "埋め込む"
    Users ||--o{ Likes : ""
    Articles ||--o{ Likes : ""
    Users ||--o{ CourseLikes : ""
    Courses ||--o{ CourseLikes : ""
    Users ||--o{ Comments : ""
    Articles ||--o{ Comments : ""
    Users ||--o{ Follows : "フォローする (FollowerId)"
    Users ||--o{ Follows : "フォローされる (FollowingId)"
    Users ||--o{ Notifications : ""
    Articles ||--o{ Notifications : ""
    Users ||--o{ EmailVerifications : ""
    Users ||--o{ PasswordResetTokens : ""

    Users {
        uuid Id PK
        string Name "max 50"
        string Email UK "正規化して保存"
        string PasswordHash "BCrypt"
        string ProfileImage "nullable"
        string Bio "nullable"
        bool IsEmailVerified
        string SecurityStamp "nullable"
        datetime AlreadyRegisteredNotifiedAt "nullable"
    }
    Articles {
        uuid Id PK
        uuid AuthorId FK
        uuid CourseId FK "nullable"
        int ArticleOrder "講座内の順番, nullable"
        string Title "max 50"
        string Body "max 10000"
        datetime PublishedAt "null は下書き, 未来は予約投稿"
    }
    Courses {
        uuid Id PK
        uuid UserId FK
        string Name "max 100"
        datetime PublishedAt "nullable"
    }
    Tags {
        uuid Id PK
        string Name UK
    }
    ArticleTags {
        int Id PK
        uuid ArticleId FK
        uuid TagId FK
    }
    Kifus {
        int Id PK
        uuid ArticleId FK
        string Name
        string KifuText "max 20000"
        int SortOrder
    }
    Likes {
        int Id PK
        uuid UserId FK
        uuid ArticleId FK
    }
    CourseLikes {
        int Id PK
        uuid UserId FK
        uuid CourseId FK
    }
    Comments {
        int Id PK
        uuid UserId FK
        uuid ArticleId FK
        string Message "max 1000"
    }
    Follows {
        int Id PK
        uuid FollowerId FK
        uuid FollowingId FK
    }
    Notifications {
        int Id PK
        uuid UserId FK
        uuid ArticleId FK
        bool IsRead
    }
    EmailVerifications {
        int Id PK
        uuid UserId FK
        string Token UK
        datetime ExpiresAt
        bool IsUsed
        string Name "サインアップ入力, nullable"
        string PasswordHash "サインアップ入力, nullable"
    }
    PasswordResetTokens {
        int Id PK
        uuid UserId FK
        string Token UK
        datetime ExpiresAt
        bool IsUsed
    }
    OutboxMessages {
        uuid Id PK
        string Type "Email / DiscordEmbed"
        string Payload "JSON"
        datetime NextAttemptAt
        int Attempts
        datetime ProcessedAt "nullable"
        datetime FailedAt "nullable"
        string LastError "nullable"
    }
```

## 補足

- 全テーブルに `CreatedAt` / `UpdatedAt` がある（`EntityBase`。未設定なら保存時に `NariNoteDbContext` が設定する）
- ID は Vogen の ValueObject（`UserId` 等）。Users / Articles / Courses / Tags は UUID v7 をアプリ側で採番し、それ以外は int の IDENTITY 列
- 外部キーはすべて `ON DELETE CASCADE`。ユーザー削除（退会・未認証ユーザー削除）で記事・講座・いいね・コメント・フォロー・通知・各種トークンがまとめて消え、講座を削除すると所属する記事も消える
- 一意制約: `Users.Email`（`lower(Email)`）、`Users.Name`、`Tags.Name`、`EmailVerifications.Token`、`PasswordResetTokens.Token`、`(ArticleId, TagId)`、`(UserId, ArticleId)` の Likes、`(UserId, CourseId)` の CourseLikes、`(FollowerId, FollowingId)` の Follows
- `OutboxMessages` は他テーブルと関連を持たない配送キュー（[architecture.md](./architecture.md#バックグラウンドジョブ)）
