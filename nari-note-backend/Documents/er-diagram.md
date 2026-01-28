# DB設計

## ER図

```mermaid
erDiagram

User ||--o{ Session: "has many"

User ||--o{ Article: "has many"
Article ||--o{ ArticleTag: "has many"
Tag ||--o{ ArticleTag: "1:n"

User ||--o{ Course: "has many"
Course ||--o{ Article: "has many"

User ||--o{ Follow: "following"
User ||--o{ Follow: "followed"

User ||--o{ Like: "has many"
Article ||--o{ Like: "has many"

User ||--o{ CourseLike: "has many"
Course ||--o{ CourseLike: "has many"

User ||--o{ Comment: "has many"
Article ||--o{ Comment: "has many"

User ||--o{ Notification: "has many"

User {
    id integer PK
    name varchar(50)
    profile_image varchar(255)
    bio varchar(500)
    email varchar(255)
    password_hash varchar(255)
    created_at datetime
    updated_at datetime
}

Session {
    id integer PK
    user_id integer FK
    session_key varchar(255)
    expires_at datetime
    created_at datetime
    updated_at datetime
}

Article {
    id integer PK
    author_id integer FK
    course_id integer FK "nullable"
    article_order integer "nullable"
    title varchar(50)
    body varchar(10000)
    is_published boolean
    created_at datetime
    updated_at datetime
}

Tag {
    id integer PK
    name varchar(50)
    created_at datetime
    updated_at datetime
}

ArticleTag {
    id integer PK
    article_id integer FK
    tag_id integer FK
    created_at datetime
    updated_at datetime
}

Course {
    id integer PK
    user_id integer FK
    name varchar(100)
    created_at datetime
    updated_at datetime
}

CourseLike {
    id integer PK
    user_id integer FK
    course_id integer FK
    created_at datetime
    updated_at datetime
}

Like {
    id integer PK
    user_id integer FK
    article_id integer FK
    created_at datetime
    updated_at datetime
}

Comment {
    id integer PK
    user_id integer FK
    article_id integer FK
    message text
    created_at datetime
    updated_at datetime
}

Follow {
    id integer PK
    follower_id integer FK
    following_id integer FK
    created_at datetime
    updated_at datetime
}

Notification {
    id integer PK
    user_id integer FK
    article_id integer FK
    is_read boolean
    created_at datetime
    updated_at datetime
}
```

## インデックス・制約

### User
- UNIQUE INDEX: `email`

### Tag
- UNIQUE INDEX: `name`

### Article
- INDEX: `author_id` (外部キー制約により自動生成)
- INDEX: `course_id` (外部キー制約により自動生成)
- INDEX: `created_at` (時系列ソート用)
- INDEX: `(is_published, published_at, created_at)` (公開記事の検索最適化用)

### ArticleTag
- UNIQUE INDEX: `(article_id, tag_id)`
- INDEX: `tag_id` (外部キー制約により自動生成)

### Course
- INDEX: `user_id` (外部キー制約により自動生成)
- INDEX: `created_at` (時系列ソート用)

### CourseLike
- UNIQUE INDEX: `(user_id, course_id)`
- INDEX: `course_id` (外部キー制約により自動生成)
- INDEX: `(user_id, created_at)` (ユーザーの講座いいね一覧取得用)

### Like
- UNIQUE INDEX: `(user_id, article_id)`
- INDEX: `article_id` (外部キー制約により自動生成)
- INDEX: `(user_id, created_at)` (ユーザーのいいね記事一覧取得用)

### Comment
- INDEX: `article_id` (外部キー制約により自動生成)
- INDEX: `user_id` (外部キー制約により自動生成)
- INDEX: `(article_id, created_at)` (記事別コメント一覧取得最適化用)

### Follow
- UNIQUE INDEX: `(follower_id, following_id)`
- INDEX: `follower_id` (フォロー中ユーザー検索用)
- INDEX: `following_id` (フォロワー検索用)

### Notification
- INDEX: `user_id` (外部キー制約により自動生成)
- INDEX: `article_id` (外部キー制約により自動生成)
- INDEX: `(user_id, is_read, created_at)` (未読通知取得最適化用)
