# ユーザー認証戦略

このドキュメントは、nari-note-backendのユーザー認証戦略について説明します。

## 目次

1. [認証方式](#認証方式)
2. [アーキテクチャ](#アーキテクチャ)
3. [実装詳細](#実装詳細)
4. [セキュリティ対策](#セキュリティ対策)
5. [APIエンドポイント](#apiエンドポイント)

---

## 認証方式

### JWT（JSON Web Token）ベース認証

nari-noteでは、**JWT（JSON Web Token）**を使用したステートレスな認証方式を採用しています。

#### 採用理由

1. **スケーラビリティ**: サーバー側でセッション状態を保持する必要がないため、水平スケーリングが容易
2. **マイクロサービス対応**: 将来的に複数のサービスに分割する際も、トークンベースで認証情報を共有可能
3. **モバイル/SPAフレンドリー**: Next.jsなどのSPAやモバイルアプリとの統合が容易

### パスワードハッシュ化

パスワードは**BCrypt**アルゴリズムを使用してハッシュ化されます。

- **BCrypt.Net-Next** ライブラリを使用
- ソルト付きハッシュで安全性を確保
- レインボーテーブル攻撃に対する耐性

---

## アーキテクチャ

```
┌─────────────┐
│   Client    │
│ (Browser/   │
│   Mobile)   │
└──────┬──────┘
       │ 1. POST /api/auth/signup or /api/auth/signin
       │    (email, password)
       ↓
┌─────────────────────────────────────┐
│        AuthController               │
│  - SignUp / SignIn エンドポイント   │
└──────────────┬──────────────────────┘
               │ 2. Request validation
               ↓
┌─────────────────────────────────────┐
│     SignUpService / SignInService   │
│  - ビジネスロジック実行             │
│  - パスワード検証・ハッシュ化       │
└──────┬────────────┬─────────────────┘
       │            │
       │ 3. User    │ 4. Session
       │    CRUD    │    CRUD
       ↓            ↓
┌─────────────┐ ┌─────────────┐
│    User     │ │   Session   │
│ Repository  │ │ Repository  │
└──────┬──────┘ └──────┬──────┘
       │                │
       └────────┬───────┘
                │ 5. Database operations
                ↓
         ┌─────────────┐
         │ PostgreSQL  │
         │   Database  │
         └─────────────┘
                │
                │ 6. JWT Token + Session created
                ↓
         ┌─────────────┐
         │   Client    │
         │ (Token保存) │
         └─────────────┘
```

---

## 実装詳細

### 1. ユーザー登録（SignUp）

#### フロー

1. クライアントが `POST /api/auth/signup` にリクエスト
2. `SignUpService` がメールアドレスの重複をチェック
3. パスワードを `BCrypt` でハッシュ化
4. ユーザーを `Users` テーブルに保存
5. JWT トークンを生成
6. セッションを `Sessions` テーブルに保存
7. トークンとユーザー情報をクライアントに返却

#### コード例

```csharp
// POST /api/auth/signup
{
  "name": "山田太郎",
  "email": "yamada@example.com",
  "password": "securePassword123"
}

// Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-29T10:00:00Z"
}
```

### 2. ログイン（SignIn）

#### フロー

1. クライアントが `POST /api/auth/signin` にリクエスト
2. `SignInService` がメールアドレスでユーザーを検索
3. パスワードを `BCrypt.Verify` で検証
4. JWT トークンを生成
5. セッションを `Sessions` テーブルに保存
6. トークンとユーザー情報をクライアントに返却

#### コード例

```csharp
// POST /api/auth/signin
{
  "email": "yamada@example.com",
  "password": "securePassword123"
}

// Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-29T10:00:00Z"
}
```

### 3. JWT トークンの構造

JWTトークンには以下の情報（Claims）が含まれます：

| Claim | 説明 |
|-------|------|
| `sub` | ユーザーID（Subject） |
| `email` | ユーザーのメールアドレス |
| `name` | ユーザー名 |
| `jti` | トークンの一意識別子（JWT ID） |
| `iss` | 発行者（Issuer） |
| `aud` | 対象者（Audience） |
| `exp` | 有効期限（Expiration） |

### 4. セッション管理

JWTトークンに加えて、データベースに `Session` レコードを保存します。

#### Sessionテーブルの役割

- **セッション無効化**: ログアウト時にセッションを削除することで、トークンを無効化
- **デバイス管理**: ユーザーがどのデバイスでログインしているかを追跡
- **セキュリティ**: 不審なアクティビティがあった場合、全セッションを削除可能

#### Session エンティティ

```csharp
public class Session
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SessionKey { get; set; }  // ランダム生成された一意のキー
    public DateTime ExpiresAt { get; set; } // 有効期限
    public DateTime CreatedAt { get; set; }
    public User User { get; set; }
}
```

---

## セキュリティ対策

### 1. パスワードセキュリティ

- ✅ **BCryptハッシュ化**: ソルト付きハッシュで保存
- ✅ **最小文字数制限**: 8文字以上（`SignUpRequest` でバリデーション）
- ✅ **平文パスワード非保存**: データベースにはハッシュのみ保存

### 2. JWT セキュリティ

- ✅ **HMAC SHA256署名**: トークンの改ざんを防止
- ✅ **有効期限設定**: デフォルト24時間で自動失効
- ✅ **Secret鍵の環境変数管理**: `appsettings.json` で設定（本番環境では環境変数推奨）

### 3. セッションセキュリティ

- ✅ **セッションキーのランダム生成**: 32バイトのランダムバイト列をBase64エンコード
- ✅ **有効期限管理**: 期限切れセッションは自動削除可能（`DeleteExpiredSessionsAsync`）
- ✅ **ユーザー単位での全セッション削除**: セキュリティインシデント時に対応可能

### 4. API セキュリティ

- ✅ **入力バリデーション**: Data Annotationsによる厳格なバリデーション
- ✅ **エラーメッセージの汎用化**: 認証エラー時は「メールアドレスまたはパスワードが正しくありません」と表示（アカウント列挙攻撃対策）
- ✅ **HTTPS強制**: `UseHttpsRedirection()` で暗号化通信を強制

### 5. データベースセキュリティ

- ✅ **Unique制約**: メールアドレスとセッションキーに一意性制約
- ✅ **外部キー制約**: データ整合性の保証
- ✅ **カスケード削除**: ユーザー削除時に関連セッションも自動削除

---

## APIエンドポイント

### POST /api/auth/signup

ユーザー登録

**Request Body**
```json
{
  "name": "山田太郎",
  "email": "yamada@example.com",
  "password": "securePassword123"
}
```

**Response (200 OK)**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-29T10:00:00Z"
}
```

**エラーレスポンス**
- `400 Bad Request`: バリデーションエラー
- `409 Conflict`: メールアドレス重複

### POST /api/auth/signin

ログイン

**Request Body**
```json
{
  "email": "yamada@example.com",
  "password": "securePassword123"
}
```

**Response (200 OK)**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "yamada@example.com",
  "name": "山田太郎",
  "expiresAt": "2025-12-29T10:00:00Z"
}
```

**エラーレスポンス**
- `400 Bad Request`: バリデーションエラー
- `401 Unauthorized`: 認証失敗（メールアドレスまたはパスワード不正）

---

## 設定

### appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-secret-key-here-minimum-32-characters-required",
    "Issuer": "nari-note-api",
    "Audience": "nari-note-client",
    "ExpirationInHours": 24
  }
}
```

### 本番環境での推奨設定

1. **JWT Secret**: 環境変数で管理（最低32文字以上のランダム文字列）
2. **HTTPS**: 必須
3. **CORS**: フロントエンドのドメインのみ許可
4. **Rate Limiting**: ブルートフォース攻撃対策

---

## 今後の拡張予定

### 1. OAuth認証（Google）

仕様書（`signin.spec`、`signup.spec`）に記載されている Google 認証の実装

- Google OAuth 2.0 を使用
- ソーシャルログイン対応

### 2. リフレッシュトークン

長期間のセッション維持のためのリフレッシュトークン実装

### 3. パスワードリセット

パスワード忘れ機能の実装（メール送信を含む）

### 4. 二要素認証（2FA）

セキュリティ強化のための2FA対応

---

## まとめ

nari-noteの認証戦略は以下の特徴を持ちます：

✅ **JWT ベースのステートレス認証**  
✅ **BCrypt によるパスワードハッシュ化**  
✅ **セッション管理によるセキュリティ強化**  
✅ **拡張可能な設計（OAuth対応予定）**  

この設計により、セキュアでスケーラブルな認証システムを実現しています。
