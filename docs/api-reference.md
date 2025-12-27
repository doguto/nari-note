# API リファレンス

このドキュメントは、nari-note-backend APIの全エンドポイントの仕様を記載します。

## 目次

1. [基本情報](#基本情報)
2. [認証](#認証)
3. [エンドポイント一覧](#エンドポイント一覧)
4. [エラーレスポンス](#エラーレスポンス)

---

## 基本情報

### ベースURL

```
http://localhost:5243
```

### Content-Type

すべてのリクエスト・レスポンスは `application/json` 形式です。

### 日付時刻フォーマット

ISO 8601形式（UTC）: `2024-01-15T10:30:00Z`

---

## 認証

現在は認証機能未実装。今後、セッションベース認証を追加予定。

---

## エンドポイント一覧

### ヘルスチェック

#### アプリケーションヘルスチェック

```
GET /health
```

**説明:** アプリケーションの健全性を確認

**レスポンス:**
- `200 OK`: アプリケーションは正常

---

#### APIヘルスチェック

```
GET /api/Health
```

**説明:** API層の健全性を確認

**レスポンス:**
```json
{
  "status": "Healthy",
  "message": "API is healthy"
}
```

---

### 記事（Articles）

#### 記事を作成

```
POST /api/articles
```

**説明:** 新しい記事を作成

**リクエストボディ:**
```json
{
  "title": "記事のタイトル",
  "body": "記事の本文",
  "authorId": 1,
  "tags": ["tag1", "tag2"],
  "isPublished": false
}
```

**フィールド:**
- `title` (string, required): 記事のタイトル（最大200文字）
- `body` (string, required): 記事の本文
- `authorId` (integer, required): 著者のユーザーID（1以上）
- `tags` (array of string, optional): タグのリスト
- `isPublished` (boolean, optional): 公開状態（デフォルト: false）

**成功レスポンス:**
- **ステータスコード:** `201 Created`
- **レスポンスボディ:**
```json
{
  "id": 1,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**エラーレスポンス:**
- **400 Bad Request**: バリデーションエラー
```json
{
  "errors": {
    "Title": ["タイトルは必須です"],
    "AuthorId": ["著者IDは1以上の値を指定してください"]
  }
}
```

- **404 Not Found**: 著者が存在しない（実装により異なる）
- **409 Conflict**: 同じタイトルの記事が既に存在
- **500 Internal Server Error**: データベースエラー

---

#### 記事を取得

```
GET /api/articles/{id}
```

**説明:** 指定されたIDの記事を取得

**パスパラメータ:**
- `id` (integer, required): 記事のID

**成功レスポンス:**
- **ステータスコード:** `200 OK`
- **レスポンスボディ:**
```json
{
  "id": 1,
  "authorId": 1,
  "title": "記事のタイトル",
  "body": "記事の本文",
  "isPublished": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:00:00Z",
  "author": {
    "id": 1,
    "name": "ユーザー名",
    "email": "user@example.com",
    "profileImage": "https://example.com/image.jpg",
    "bio": "プロフィール",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "articleTags": [],
  "likes": []
}
```

**エラーレスポンス:**
- **404 Not Found**: 記事が存在しない
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Article with ID 999 not found",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/999",
    "additionalData": null
  }
}
```

---

#### 著者の記事一覧を取得

```
GET /api/articles/author/{authorId}
```

**説明:** 指定された著者の記事一覧を取得（作成日時の降順）

**パスパラメータ:**
- `authorId` (integer, required): 著者のユーザーID

**成功レスポンス:**
- **ステータスコード:** `200 OK`
- **レスポンスボディ:**
```json
{
  "articles": [
    {
      "id": 2,
      "authorId": 1,
      "title": "記事2",
      "body": "本文2",
      "isPublished": true,
      "createdAt": "2024-01-15T12:00:00Z",
      "updatedAt": "2024-01-15T12:00:00Z",
      "author": {
        "id": 1,
        "name": "ユーザー名"
      }
    },
    {
      "id": 1,
      "authorId": 1,
      "title": "記事1",
      "body": "本文1",
      "isPublished": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "author": {
        "id": 1,
        "name": "ユーザー名"
      }
    }
  ]
}
```

**エラーレスポンス:**
- **404 Not Found**: ユーザーが存在しない（実装により異なる）

---

#### 記事を削除

```
DELETE /api/articles/{id}?userId={userId}
```

**説明:** 指定された記事を削除（著者のみ可能）

**パスパラメータ:**
- `id` (integer, required): 記事のID

**クエリパラメータ:**
- `userId` (integer, required): 削除を実行するユーザーID

**成功レスポンス:**
- **ステータスコード:** `204 No Content`
- **レスポンスボディ:** なし

**エラーレスポンス:**
- **404 Not Found**: 記事が存在しない
- **403 Forbidden**: 削除権限がない
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "You don't have permission to delete this article",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/1",
    "additionalData": null
  }
}
```

---

### ユーザー（Users）

#### ユーザープロフィールを取得

```
GET /api/users/{id}
```

**説明:** 指定されたユーザーのプロフィールを取得

**パスパラメータ:**
- `id` (integer, required): ユーザーのID

**成功レスポンス:**
- **ステータスコード:** `200 OK`
- **レスポンスボディ:**
```json
{
  "id": 1,
  "name": "ユーザー名",
  "profileImage": "https://example.com/image.jpg",
  "bio": "自己紹介文",
  "email": "user@example.com",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

**エラーレスポンス:**
- **404 Not Found**: ユーザーが存在しない
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "User with ID 999 not found",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/users/999",
    "additionalData": null
  }
}
```

---

## エラーレスポンス

すべてのエラーレスポンスは統一された形式で返却されます。

### エラーレスポンス形式

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "エラーメッセージ",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/path",
    "additionalData": {
      "key": "value"
    }
  }
}
```

### エラーコード一覧

| HTTPステータス | エラーコード | 説明 |
|--------------|------------|------|
| 400 | `VALIDATION_ERROR` | 入力値が不正 |
| 401 | `UNAUTHORIZED` | 認証が必要 |
| 403 | `FORBIDDEN` | 権限が不足 |
| 404 | `NOT_FOUND` | リソースが見つからない |
| 409 | `CONFLICT` | リソースの競合 |
| 500 | `INFRASTRUCTURE_ERROR` | インフラエラー（DB等） |
| 500 | `INTERNAL_SERVER_ERROR` | 予期しないエラー |

### エラーレスポンス例

#### 404 Not Found
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Article with ID 999 not found",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/999",
    "additionalData": null
  }
}
```

#### 400 Validation Error
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid article data",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": {
      "titleLength": 250,
      "maxLength": 200
    }
  }
}
```

#### 409 Conflict
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Article with this title already exists",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": null
  }
}
```

#### 403 Forbidden
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "You don't have permission to update this article",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/1",
    "additionalData": null
  }
}
```

#### 500 Infrastructure Error
```json
{
  "error": {
    "code": "INFRASTRUCTURE_ERROR",
    "message": "Database error occurred while creating article",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": null
  }
}
```

---

## 注意事項

### 認証について

現在は認証機能が未実装のため、`userId` などをクエリパラメータで渡しています。
今後、セッションベース認証を実装予定です。

### ページネーション

現在はページネーション未実装ですが、将来的に以下のクエリパラメータで対応予定：
- `page` (integer): ページ番号
- `pageSize` (integer): 1ページあたりの件数

### フィルタリング・ソート

今後実装予定の機能：
- タグでのフィルタリング
- 公開状態でのフィルタリング
- 作成日時/更新日時でのソート

---

## 開発者向け情報

### テスト用curl例

#### 記事作成
```bash
curl -X POST http://localhost:5243/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "title": "テスト記事",
    "body": "これはテスト記事です",
    "authorId": 1,
    "isPublished": true
  }'
```

#### 記事取得
```bash
curl http://localhost:5243/api/articles/1
```

#### 著者の記事一覧
```bash
curl http://localhost:5243/api/articles/author/1
```

#### 記事削除
```bash
curl -X DELETE "http://localhost:5243/api/articles/1?userId=1"
```

#### ユーザープロフィール取得
```bash
curl http://localhost:5243/api/users/1
```

---

## 関連ドキュメント

- [実装ガイド](/docs/backend-implementation-guide.md) - 実装パターンとコーディング規約
- [開発ワークフロー](/docs/development-workflow.md) - 開発手順とタスクガイド
- [エラーハンドリング戦略](/docs/error-handling-strategy.md) - エラーハンドリングの包括的ガイド
