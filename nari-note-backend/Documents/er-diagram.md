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
    Users ||--o{ Playbooks : "作る"
    Playbooks ||--o{ PlaybookNodes : ""
    Playbooks ||--o{ PlaybookNodeBoards : ""
    Playbooks ||--o{ PlaybookNodeSteps : ""
    PlaybookNodes ||--o{ PlaybookNodeBoards : "まとめる"
    PlaybookNodeBoards ||--o{ PlaybookNodeSteps : "出発 (FromBoardId)"
    PlaybookNodeBoards |o--o{ PlaybookNodeSteps : "行き先 (ToBoardId)"

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
    Playbooks {
        uuid Id PK
        uuid UserId FK
        string Name "max 100"
        string RootSfen "開始局面"
    }
    PlaybookNodes {
        int Id PK
        uuid PlaybookId FK
        string Name "max 100"
        string Memo "研究メモ, max 10000"
    }
    PlaybookNodeBoards {
        uuid Id PK "登録順に大きくなる UUID v7"
        uuid PlaybookId FK
        int NodeId FK
        string Sfen "手数を 1 に正規化, (PlaybookId, Sfen) で一意"
    }
    PlaybookNodeSteps {
        int Id PK
        uuid PlaybookId FK
        uuid FromBoardId FK
        uuid ToBoardId FK "nullable。終点がノードでない手順"
        text_array Usis "手順"
        string Condition "まとめた盤面で行き先が分かれるときの条件, max 100"
    }
```

## 補足

- 全テーブルに `CreatedAt` / `UpdatedAt` がある（`EntityBase`。未設定なら保存時に `NariNoteDbContext` が設定する）
- ID は Vogen の ValueObject（`UserId` 等）。Users / Articles / Courses / Tags / Playbooks / PlaybookNodeBoards は UUID v7 をアプリ側で採番し、それ以外は int の IDENTITY 列。PlaybookNodeBoards は同じミリ秒の中でも登録順に大きくなるよう採番する
- 外部キーは `PlaybookNodeBoards.NodeId`（`NO ACTION`）以外すべて `ON DELETE CASCADE`。ユーザー削除（退会・未認証ユーザー削除）で記事・講座・いいね・コメント・フォロー・通知・各種トークンがまとめて消え、講座を削除すると所属する記事も消える
- 一意制約: `Users.Email`（`lower(Email)`）、`Users.Name`、`Tags.Name`、`EmailVerifications.Token`、`PasswordResetTokens.Token`、`(ArticleId, TagId)`、`(UserId, ArticleId)` の Likes、`(UserId, CourseId)` の CourseLikes、`(FollowerId, FollowingId)` の Follows
- `OutboxMessages` は他テーブルと関連を持たない配送キュー（[architecture.md](./architecture.md#バックグラウンドジョブ)）
- マイ定跡: 開始局面・分岐点・手順前後の合流点・名前かメモを付けた局面だけを盤面（`PlaybookNodeBoards`）として持ち、その間の手は手順（`PlaybookNodeSteps.Usis`）にまとめる。途中の局面は行を持たず、手順を指し直して求める。手順は出発の盤面と 1 手目で一意。似た盤面をまとめると 1 つのノード（`PlaybookNodes`）に複数の盤面が属し、名前と研究メモを共有する。ノードの代表は最小 ID（最初に登録した）の盤面
