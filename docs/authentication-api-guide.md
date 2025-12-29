# 認証API使用ガイド

## 概要

このドキュメントは、nari-note-backendの認証API（SignUp/SignIn）の使用方法を説明します。

## エンドポイント

### 1. ユーザー登録（SignUp）

新規ユーザーを登録します。

**エンドポイント**: `POST /api/auth/signup`

**リクエストボディ**:
```json
{
  "name": "山田太郎",
  "email": "yamada@example.com",
  "password": "securePassword123"
}
```

**バリデーション**:
- `name`: 必須、最大50文字
- `email`: 必須、有効なメールアドレス形式、最大255文字
- `password`: 必須、最小8文字、最大255文字

**成功時のレスポンス（200 OK）**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-30T13:00:00Z"
}
```

**エラーレスポンス**:

- **400 Bad Request**: バリデーションエラー
  ```json
  {
    "errors": {
      "Email": ["有効なメールアドレスを入力してください"],
      "Password": ["パスワードは8文字以上で入力してください"]
    }
  }
  ```

- **409 Conflict**: メールアドレス重複
  ```json
  {
    "error": {
      "code": "CONFLICT",
      "message": "このメールアドレスは既に使用されています",
      "timestamp": "2025-12-29T13:00:00Z",
      "path": "/api/auth/signup"
    }
  }
  ```

### 2. ログイン（SignIn）

既存ユーザーでログインします。

**エンドポイント**: `POST /api/auth/signin`

**リクエストボディ**:
```json
{
  "email": "yamada@example.com",
  "password": "securePassword123"
}
```

**バリデーション**:
- `email`: 必須、有効なメールアドレス形式
- `password`: 必須

**成功時のレスポンス（200 OK）**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-30T13:00:00Z"
}
```

**エラーレスポンス**:

- **400 Bad Request**: バリデーションエラー
  ```json
  {
    "errors": {
      "Email": ["有効なメールアドレスを入力してください"]
    }
  }
  ```

- **401 Unauthorized**: 認証失敗
  ```json
  {
    "error": {
      "code": "UNAUTHORIZED",
      "message": "メールアドレスまたはパスワードが正しくありません",
      "timestamp": "2025-12-29T13:00:00Z",
      "path": "/api/auth/signin"
    }
  }
  ```

## 認証が必要なAPIの使用方法

SignUp/SignInで取得したトークンを使用して、他のAPIにアクセスします。

**リクエスト例**:
```http
GET /api/articles/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**認証エラーレスポンス（401 Unauthorized）**:
```json
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "認証が必要です",
    "timestamp": "2025-12-29T13:00:00Z",
    "path": "/api/articles/1"
  }
}
```

## トークンの有効期限

- デフォルト: 24時間
- 設定ファイル（`appsettings.json`）で変更可能: `Jwt:ExpirationInHours`

## セキュリティ機能

1. **パスワードハッシュ化**: BCryptアルゴリズムでソルト付きハッシュ化
2. **JWT署名**: HMAC SHA256で署名
3. **セッション管理**: データベースでセッションを管理し、無効化可能
4. **入力バリデーション**: Data Annotationsによる厳格なバリデーション
5. **エラーメッセージの汎用化**: アカウント列挙攻撃への対策

## テスト用コマンド例（curl）

### SignUp
```bash
curl -X POST http://localhost:5243/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "name": "山田太郎",
    "email": "yamada@example.com",
    "password": "securePassword123"
  }'
```

### SignIn
```bash
curl -X POST http://localhost:5243/api/auth/signin \
  -H "Content-Type: application/json" \
  -d '{
    "email": "yamada@example.com",
    "password": "securePassword123"
  }'
```

### 認証が必要なAPIへのアクセス
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
curl -X GET http://localhost:5243/api/articles/1 \
  -H "Authorization: Bearer $TOKEN"
```

## 注意事項

1. **本番環境での秘密鍵管理**
   - `appsettings.json`の`Jwt:Secret`は環境変数で管理してください
   - 最低32文字以上のランダムな文字列を使用してください

2. **HTTPS通信**
   - 本番環境では必ずHTTPSを使用してください
   - トークンが平文で送信されるのを防ぎます

3. **トークンの保管**
   - Webブラウザでは、HttpOnly Cookieでの保管を推奨（将来実装予定）
   - モバイルアプリでは、Secure Storage（iOS: Keychain、Android: EncryptedSharedPreferences）を使用してください
