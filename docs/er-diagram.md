# DB設計

## ER図

```mermaid
erDiagram

User ||--o{ Session: "has many"

User ||--o{ Article: "has many"
Article ||--o{ ArticleTag: "has many"
Tag ||--o{ ArticleTag: "1:n"

User ||--o{ Follow: "following"
User ||--o{ Follow: "followed"

User ||--o{ Like: "has many"
Article ||--o{ Like: "has many"

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
}

Article {
    id integer PK
    author_id integer FK
    title varchar(50)
    body text
    is_published boolean
    created_at datetime
    updated_at datetime
}

Tag {
    id integer PK
    name varchar(50)
    created_at datetime
}

ArticleTag {
    id integer PK
    article_id integer FK
    tag_id integer FK
    created_at datetime
}

Like {
    id integer PK
    user_id integer FK
    article_id integer FK
    created_at datetime
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
    following_id integer FK
    followed_id integer FK
    created_at datetime
}

Notification {
    id integer PK
    user_id integer FK
    article_id integer FK
    is_read boolean
    created_at datetime
}
```

## インデックス・制約

### ArticleTag
- UNIQUE INDEX: `(article_id, tag_id)`
- INDEX: `(user_id)`

### Like
- UNIQUE INDEX: `(user_id, article_id)`

### Follow
- UNIQUE INDEX: `(following_id, followed_id)`
