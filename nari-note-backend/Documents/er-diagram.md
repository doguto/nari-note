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
    Users ||--o{ Books : "作る"
    Books ||--o{ BookPositions : ""
    Books ||--o{ BookMoves : ""
    Books ||--o{ ShapeGroups : ""
    Books ||--o{ ShapeCandidates : ""
    ShapeGroups ||--o{ BookPositions : "まとめる"
    BookPositions ||--o{ BookMoves : "親 (ParentPositionId)"
    BookPositions ||--o{ BookMoves : "子 (ChildPositionId)"
    BookPositions ||--o{ ShapeCandidates : "A / B"

    Users {
        uuid Id PK
        string Name "max 50"
        string Email UK "正規化して保存"
        string PasswordHash "BCrypt"
        string ProfileImage "nullable"
        string Bio "nullable"
        string XAccountName "nullable"
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
    Books {
        int Id PK
        uuid UserId FK
        string Name "max 100"
        string RootSfen "開始局面"
    }
    BookPositions {
        int Id PK
        int BookId FK
        string Sfen "手数を 1 に正規化, (BookId, Sfen) で一意"
        int ShapeGroupId FK
        bool CandidatesScanned "同じ形の候補を探し終えたか"
    }
    BookMoves {
        int Id PK
        int BookId FK
        int ParentPositionId FK
        int ChildPositionId FK
        string Usi "(ParentPositionId, Usi) で一意"
        string Label "記譜"
        string Memo "max 200"
    }
    ShapeGroups {
        int Id PK
        int BookId FK
        string Name "max 100"
        string Memo "研究メモ, max 10000"
    }
    ShapeCandidates {
        int Id PK
        int BookId FK
        int PositionAId FK
        int PositionBId FK
        string Status "Queued / Running / Done / Failed"
        string Verdict "same / same_with_delta / same_until / different / skipped"
        int Delta "nullable"
        int ConfirmedDepth "nullable"
        string Decision "Pending / Merged / Rejected"
    }
```

## 補足

- 全テーブルに `CreatedAt` / `UpdatedAt` がある（`EntityBase`。未設定なら保存時に `NariNoteDbContext` が設定する）
- ID は Vogen の ValueObject（`UserId` 等）。Users / Articles / Courses / Tags は UUID v7 をアプリ側で採番し、それ以外は int の IDENTITY 列
- 外部キーは `BookPositions.ShapeGroupId`（`NO ACTION`）以外すべて `ON DELETE CASCADE`。ユーザー削除（退会・未認証ユーザー削除）で記事・講座・いいね・コメント・フォロー・通知・各種トークンがまとめて消え、講座を削除すると所属する記事も消える
- 一意制約: `Users.Email`（`lower(Email)`）、`Users.Name`、`Tags.Name`、`EmailVerifications.Token`、`PasswordResetTokens.Token`、`(ArticleId, TagId)`、`(UserId, ArticleId)` の Likes、`(UserId, CourseId)` の CourseLikes、`(FollowerId, FollowingId)` の Follows
- `OutboxMessages` は他テーブルと関連を持たない配送キュー（[architecture.md](./architecture.md#バックグラウンドジョブ)）
- マイ定跡: 局面（`BookPositions`）と手（`BookMoves`）が定跡のグラフになる。手順前後で同じ局面になれば同じ行に合流する。本質的に同じ形とした局面は `ShapeGroups` でまとめ、研究メモを共有する。まとめていない局面も 1 局面だけのグループに属し、グループの代表局面は最小 ID の局面。`ShapeCandidates` は `ShapeJudgeWorker` がエンジンサービスで判定した結果で、まとめるかは本人が決める
