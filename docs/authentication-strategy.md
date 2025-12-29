# ユーザー認証戦略

このドキュメントは、nari-note-backendのユーザー認証戦略について説明します。

## 目次

1. [検討した認証方式](#検討した認証方式)
2. [選定結果と理由](#選定結果と理由)
3. [XSS（クロスサイトスクリプティング）対策](#xssクロスサイトスクリプティング対策)
4. [アーキテクチャ](#アーキテクチャ)
5. [実装詳細](#実装詳細)
6. [セキュリティ対策](#セキュリティ対策)
7. [APIエンドポイント](#apiエンドポイント)

---

## 検討した認証方式

### 1. セッションベース認証（Cookie）

**概要**
- サーバー側でセッション情報を管理
- クライアントにはセッションIDのみをCookieで保持
- 伝統的なWebアプリケーションで広く使用される方式

**メリット**
- ✅ セッション無効化が即座に可能
- ✅ サーバー側で完全な制御が可能
- ✅ 実装が比較的シンプル
- ✅ ASP.NET Core標準機能で実装可能

**デメリット**
- ❌ サーバー側でセッション状態を保持する必要がある
- ❌ 水平スケーリング時にRedis等の外部ストアが必要
- ❌ モバイルアプリとの統合が複雑
- ❌ CORS設定が複雑になる可能性
- ❌ マイクロサービスアーキテクチャに不向き

**採用判断**: ❌ **不採用**

### 2. JWT（JSON Web Token）ベース認証

**概要**
- トークンに認証情報を含める自己完結型の認証方式
- サーバー側でセッション状態を保持しないステートレス認証
- 署名により改ざん検知が可能

**メリット**
- ✅ ステートレスでスケーラビリティが高い
- ✅ マイクロサービスアーキテクチャに適している
- ✅ SPA/モバイルアプリとの統合が容易
- ✅ CORS問題が発生しにくい
- ✅ サーバー側でセッションストアが不要

**デメリット**
- ❌ トークンの即座の無効化が困難（有効期限まで有効）
- ❌ トークンサイズが大きい
- ❌ リフレッシュトークンの実装が必要
- ⚠️ **XSS攻撃のリスク**（localStorageに保存する場合）

**採用判断**: ✅ **採用**（ハイブリッド方式とHttpOnly Cookieで欠点を補完）

### 3. OAuth 2.0 / OpenID Connect

**概要**
- 外部認証プロバイダー（Google, GitHub等）を利用
- 認証を外部サービスに委譲

**メリット**
- ✅ ユーザーがパスワードを管理する必要がない
- ✅ 実装コストの削減
- ✅ セキュリティレベルの向上
- ✅ ソーシャルログインによるUX向上

**デメリット**
- ❌ 外部サービスへの依存
- ❌ ネットワーク遅延の影響
- ❌ プロバイダーのメンテナンス影響
- ❌ 初回ログインのみでメール/パスワードログインが使えない

**採用判断**: 🔄 **将来実装予定**（補助的な認証手段として）

### 4. API Key認証

**概要**
- APIキーをリクエストヘッダーに含める方式
- 主にサーバー間通信で使用

**メリット**
- ✅ 実装がシンプル
- ✅ 機械間通信に適している

**デメリット**
- ❌ ユーザー認証には不向き
- ❌ キーの漏洩リスク
- ❌ ローテーションが困難
- ❌ ユーザー体験が悪い

**採用判断**: ❌ **不採用**（ユーザー認証には不適切）

### 5. Basic認証

**概要**
- HTTPヘッダーにBase64エンコードしたユーザー名とパスワードを含める
- HTTP標準の認証方式

**メリット**
- ✅ 実装が極めてシンプル
- ✅ 標準化されている

**デメリット**
- ❌ 毎回パスワードを送信する必要がある
- ❌ セキュリティレベルが低い
- ❌ ログアウト機能が実装困難
- ❌ 現代的なWebアプリケーションには不適切

**採用判断**: ❌ **不採用**（セキュリティ要件を満たさない）

---

## 選定結果と理由

### 採用方式: **JWTベース認証 + セッション管理のハイブリッド方式 + HttpOnly Cookie**

nari-noteでは、**JWT（JSON Web Token）**を主要な認証方式として採用し、セッション管理を組み合わせ、さらに**XSS対策としてHttpOnly Cookieを使用**したハイブリッド方式を実装します。

#### 選定理由

1. **スケーラビリティの確保**
   - ステートレス認証により、サーバーの水平スケーリングが容易
   - セッションストアへの依存を最小化

2. **マイクロサービス対応**
   - 将来的に複数のサービスに分割する際も、トークンベースで認証情報を共有可能
   - サービス間通信にも同じトークンを利用可能

3. **モバイル/SPA対応**
   - Next.jsなどのSPAやモバイルアプリとの統合が容易
   - CORS問題が発生しにくい

4. **JWTの欠点を補完**
   - データベースにセッションレコードを保持することで、トークンの即座の無効化を実現
   - ログアウト機能やセキュリティインシデント時の全セッション削除が可能

5. **XSS攻撃への対策**
   - **HttpOnly Cookie**を使用してJWTトークンを保存
   - JavaScriptからトークンへのアクセスを防止
   - CSRF対策も併用

6. **将来の拡張性**
   - OAuth 2.0（Google認証等）との統合が容易
   - リフレッシュトークンの実装も可能

---

## XSS（クロスサイトスクリプティング）対策

### 問題: JWTトークンのXSS脆弱性

JWTトークンをブラウザで扱う際、以下のような保存方法にはXSS攻撃のリスクがあります：

#### ❌ **脆弱な保存方法**

1. **localStorage / sessionStorage**
   ```javascript
   // 危険な例
   localStorage.setItem('token', jwtToken);
   ```
   - JavaScriptから自由にアクセス可能
   - XSS攻撃によりトークンが盗まれる
   - 攻撃者がトークンを取得してなりすまし可能

2. **通常のCookie（HttpOnly なし）**
   ```javascript
   // 危険な例
   document.cookie = `token=${jwtToken}`;
   ```
   - JavaScriptから読み取り可能
   - XSS攻撃でトークンが漏洩

### 解決策: HttpOnly Cookie + 追加のセキュリティ対策

#### ✅ **推奨する実装方式**

1. **HttpOnly Cookie の使用**
   
   サーバー側でJWTトークンをHttpOnly Cookieに設定：
   
   ```csharp
   // C# (ASP.NET Core) での実装例
   Response.Cookies.Append("authToken", jwtToken, new CookieOptions
   {
       HttpOnly = true,        // JavaScriptからアクセス不可
       Secure = true,          // HTTPS のみ
       SameSite = SameSiteMode.Strict,  // CSRF対策
       MaxAge = TimeSpan.FromHours(24)
   });
   ```
   
   **メリット:**
   - ✅ JavaScriptからトークンにアクセスできない
   - ✅ XSS攻撃でトークンが盗まれない
   - ✅ ブラウザが自動的にCookieを送信

2. **SameSite属性によるCSRF対策**
   
   ```csharp
   SameSite = SameSiteMode.Strict  // または Lax
   ```
   
   - `Strict`: 同一サイトからのリクエストのみCookieを送信
   - `Lax`: GET リクエストなど一部のクロスサイトリクエストを許可
   - CSRF攻撃を防止

3. **Secure属性の設定**
   
   ```csharp
   Secure = true
   ```
   
   - HTTPS通信のみでCookieを送信
   - 中間者攻撃（MITM）を防止

4. **短い有効期限 + リフレッシュトークン**
   
   - アクセストークン: 15分〜1時間程度の短い有効期限
   - リフレッシュトークン: より長い有効期限（7日〜30日）
   - トークンが漏洩した場合の影響を最小化

### トークン保存方式の比較

| 保存方式 | XSS脆弱性 | CSRF脆弱性 | モバイル対応 | 推奨度 |
|---------|----------|-----------|------------|--------|
| localStorage | ❌ 高リスク | ✅ 安全 | ✅ 容易 | ❌ 非推奨 |
| sessionStorage | ❌ 高リスク | ✅ 安全 | ✅ 容易 | ❌ 非推奨 |
| 通常のCookie | ❌ 高リスク | ❌ 高リスク | ⚠️ 複雑 | ❌ 非推奨 |
| **HttpOnly Cookie** | ✅ **安全** | ⚠️ 要対策 | ⚠️ 複雑 | ✅ **推奨** |
| HttpOnly + SameSite | ✅ **安全** | ✅ **安全** | ⚠️ 複雑 | ✅ **最推奨** |

### その他のXSS対策

1. **Content Security Policy (CSP)**
   
   HTTPヘッダーでスクリプトの実行を制限：
   
   ```csharp
   app.Use(async (context, next) =>
   {
       context.Response.Headers.Add("Content-Security-Policy", 
           "default-src 'self'; script-src 'self'");
       await next();
   });
   ```

2. **入力のサニタイゼーション**
   
   - ユーザー入力を常にエスケープ
   - HTMLタグの無効化
   - ASP.NET Core の Razor Pages は自動エスケープ機能あり

3. **出力のエンコーディング**
   
   - HTML、JavaScript、URL等のコンテキストに応じた適切なエンコーディング
   - `HtmlEncoder.Default.Encode()` などを使用

4. **定期的なセキュリティ監査**
   
   - 依存ライブラリの脆弱性チェック
   - OWASP Top 10 に基づくセキュリティレビュー

### モバイルアプリケーションの場合

モバイルアプリ（iOS, Android）では、ブラウザのCookieが使用できないため、以下の方式を採用：

1. **Secure Storage の使用**
   - iOS: Keychain
   - Android: EncryptedSharedPreferences
   - トークンを暗号化して保存

2. **Certificate Pinning**
   - HTTPS通信のセキュリティ強化
   - 中間者攻撃を防止

### 実装時の注意点

1. **Webアプリケーション（ブラウザ）**
   - ✅ HttpOnly Cookie + SameSite属性を使用
   - ✅ HTTPS必須
   - ✅ CSP ヘッダー設定

2. **モバイルアプリケーション**
   - ✅ Secure Storage に保存
   - ✅ Certificate Pinning 実装
   - ⚠️ HttpOnly Cookie は使用不可（代替手段を使用）

3. **API間通信（サーバー間）**
   - ✅ Authorization ヘッダーでトークン送信
   - ✅ 相互TLS認証の検討

### パスワードハッシュ化アルゴリズムの検討

#### 1. BCrypt ✅ **採用**

**特徴**
- 適応型ハッシュ関数（計算コストを調整可能）
- ソルト自動生成
- レインボーテーブル攻撃に強い

**メリット**
- ✅ 広く使用されている実績あり
- ✅ .NET用のライブラリ（BCrypt.Net-Next）が充実
- ✅ 計算コストを調整できる（将来的に強度を上げられる）
- ✅ ソルト管理が自動化されている

**デメリット**
- ⚠️ Argon2より若干セキュリティレベルが低い

**採用理由**: 実績と安定性、ライブラリの充実度から採用

#### 2. Argon2

**特徴**
- Password Hashing Competition 優勝アルゴリズム
- メモリ使用量とCPU使用量の両方を調整可能
- 最新の暗号学的ハッシュ関数

**メリット**
- ✅ 最も安全とされる現代的アルゴリズム
- ✅ GPUベースの攻撃に強い
- ✅ メモリとCPUの両方を使用

**デメリット**
- ❌ .NETの標準ライブラリに含まれていない
- ❌ サードパーティライブラリの選択肢が少ない
- ❌ BCryptと比較して採用実績が少ない

**採用判断**: ❌ 不採用（将来的に検討）

#### 3. PBKDF2

**特徴**
- NIST推奨のアルゴリズム
- ASP.NET Core Identityのデフォルト

**メリット**
- ✅ .NET標準ライブラリで実装可能
- ✅ FIPS準拠が必要な場合に適している

**デメリット**
- ❌ BCryptやArgon2より攻撃に弱い
- ❌ GPUベースの攻撃に脆弱

**採用判断**: ❌ 不採用（セキュリティレベルが相対的に低い）

#### 4. SHA-256/SHA-512（ソルト付き）

**特徴**
- 高速なハッシュ関数
- 暗号学的ハッシュ関数

**メリット**
- ✅ 実装がシンプル
- ✅ 高速

**デメリット**
- ❌ パスワードハッシュ用には設計されていない
- ❌ 高速すぎてブルートフォース攻撃に弱い
- ❌ 適応型でない

**採用判断**: ❌ 不採用（パスワードハッシュには不適切）

---

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
- ✅ **HttpOnly Cookie**: XSS攻撃からトークンを保護（詳細は[XSS対策](#xssクロスサイトスクリプティング対策)を参照）

### 3. XSS（クロスサイトスクリプティング）対策

- ✅ **HttpOnly Cookie**: JavaScriptからトークンへのアクセスを防止
- ✅ **Secure属性**: HTTPS通信のみでCookieを送信
- ✅ **SameSite属性**: CSRF攻撃を防止（Strict または Lax）
- ✅ **Content Security Policy (CSP)**: スクリプト実行を制限
- ✅ **入力のサニタイゼーション**: ユーザー入力を常にエスケープ
- ✅ **出力のエンコーディング**: コンテキストに応じた適切なエンコーディング

詳細は[XSS対策](#xssクロスサイトスクリプティング対策)セクションを参照してください。

### 4. CSRF（クロスサイトリクエストフォージェリ）対策

- ✅ **SameSite Cookie属性**: クロスサイトからのCookie送信を制限
- ✅ **CSRF トークン**: 重要な操作には追加のトークン検証（将来実装予定）
- ✅ **Origin/Referer ヘッダー検証**: リクエスト元の検証

### 5. セッションセキュリティ

- ✅ **セッションキーのランダム生成**: 32バイトのランダムバイト列をBase64エンコード
- ✅ **有効期限管理**: 期限切れセッションは自動削除可能（`DeleteExpiredSessionsAsync`）
- ✅ **ユーザー単位での全セッション削除**: セキュリティインシデント時に対応可能

### 6. API セキュリティ

- ✅ **入力バリデーション**: Data Annotationsによる厳格なバリデーション
- ✅ **エラーメッセージの汎用化**: 認証エラー時は「メールアドレスまたはパスワードが正しくありません」と表示（アカウント列挙攻撃対策）
- ✅ **HTTPS強制**: `UseHttpsRedirection()` で暗号化通信を強制
- ✅ **Rate Limiting**: ブルートフォース攻撃対策（将来実装予定）

### 7. データベースセキュリティ

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
