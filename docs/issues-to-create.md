# 作成すべきIssue一覧

このドキュメントは、`docs/pending-tasks-analysis.md`の調査結果に基づいて作成すべきissueの一覧です。

> **注意**: 以下のissueは優先度順に並んでいます。[BACK]はバックエンド、[FRONT]はフロントエンド、[INFRA]はインフラ、[DOCS]はドキュメント関連のissueです。

---

## 🔴 重要度：高（早急に対応が必要）

### 1. [BACK] 通知機能のバックエンド実装

**説明**:
通知機能のバックエンドAPIを実装する必要があります。現在、Notificationエンティティは定義されていますが、以下の機能が未実装です：
- フォローユーザーが記事を投稿した際の通知作成ロジック
- 通知一覧取得APIエンドポイント
- 通知既読管理APIエンドポイント

**完了条件**:
- フォローしているユーザーが記事を投稿すると、自動的に通知が作成される
- 通知一覧を取得できるAPIエンドポイントが実装されている
- 通知を既読にできるAPIエンドポイントが実装されている
- すべての通知を既読にできるAPIエンドポイントが実装されている
- 直近1ヶ月の通知のみを取得できる
- 通知一覧はページネーション対応している（20件/ページ）

**追加情報**:
- 仕様書: `spec/notification.spec`
- 関連エンティティ: `Domain/Entity/Notification.cs`
- 実装が必要なファイル:
  - `Controller/NotificationsController.cs` (新規作成)
  - `Application/Service/CreateNotificationService.cs` (新規作成)
  - `Application/Service/GetNotificationsService.cs` (新規作成)
  - `Application/Service/MarkNotificationAsReadService.cs` (新規作成)
  - `Application/Service/MarkAllNotificationsAsReadService.cs` (新規作成)
  - `Application/Service/CreateArticleService.cs` (通知作成ロジックの追加)
  - `Domain/Repository/INotificationRepository.cs` (メソッド追加)
  - `Infrastructure/Repository/NotificationRepository.cs` (新規作成または実装追加)

**ラベル**: BackEnd, Priority: High

---

### 2. [FRONT] 通知機能のフロントエンド実装

**説明**:
通知機能のフロントエンドUIを実装する必要があります。以下の機能が未実装です：
- ヘッダーに通知アイコンとバッジ表示
- 通知ドロップダウンUI
- 通知ページ（/notifications）
- 既読/未読管理UI

**完了条件**:
- ログイン時にヘッダーに通知アイコンが表示される
- 未読通知がある場合、通知アイコンにバッジが表示される
- 通知アイコンをクリックすると通知ドロップダウンが表示される
- 通知ドロップダウンには最新20件の通知が表示される
- 通知をクリックすると該当の記事ページに遷移する
- 通知ページ（/notifications）で全通知をページネーション付きで表示できる
- 通知一覧を開くと自動的に既読になる
- すべて既読にするボタンが機能する

**追加情報**:
- 仕様書: `spec/notification.spec`
- 実装が必要なファイル:
  - `src/app/(with-layout)/notifications/page.tsx` (新規作成)
  - `src/features/notification/organisms/NotificationDropdown.tsx` (新規作成)
  - `src/features/notification/organisms/NotificationList.tsx` (新規作成)
  - `src/features/global/Header.tsx` (通知アイコン追加)
  - `src/lib/api/endpoints.ts` (通知API追加)
  - `src/lib/api/hooks.ts` (通知フック追加)
  - `src/lib/api/types.ts` (通知型定義追加)

**ラベル**: FrontEnd, Priority: High

---

### 3. [BACK] Google OAuth認証の実装

**説明**:
Google OAuth 2.0認証機能を実装する必要があります。現在、メールアドレス/パスワード認証のみ実装されていますが、仕様書ではGoogle認証も要求されています。

**完了条件**:
- Google OAuth認証用のAPIエンドポイントが実装されている
- Googleの認証画面にリダイレクトできる
- Google認証後のCallbackを処理できる
- 初回認証時に新規アカウントが自動作成される
- 既存アカウントがある場合はそのアカウントでログインできる
- 認証成功後、JWTトークンが発行される

**追加情報**:
- 仕様書: `spec/signin.spec`（25-64行目）
- 実装が必要なファイル:
  - `Controller/AuthController.cs` (Googleエンドポイント追加)
  - `Application/Service/GoogleSignInService.cs` (新規作成)
  - `Application/Dto/Request/GoogleAuthRequest.cs` (新規作成)
  - `appsettings.json` (Google OAuth設定追加)
- 必要な環境変数:
  - `Google:ClientId`
  - `Google:ClientSecret`
  - `Google:RedirectUri`
- 必要なNuGetパッケージ:
  - `Google.Apis.Auth`（Google IDトークン検証用）

**ラベル**: BackEnd, Priority: High, Authentication

---

### 4. [FRONT] Google OAuth認証のフロントエンド実装

**説明**:
Google OAuth認証のフロントエンドUIを実装する必要があります。ログインページとサインアップページにGoogleサインインボタンを追加します。

**完了条件**:
- ログインページに「Googleでサインイン」ボタンが表示される
- サインアップページに「Googleでサインアップ」ボタンが表示される
- Googleサインインボタンをクリックすると、Googleの認証画面に遷移する
- Google認証完了後、ホームページにリダイレクトされる
- 認証失敗時は適切なエラーメッセージが表示される

**追加情報**:
- 仕様書: `spec/signin.spec`（8行目、25-64行目）
- 実装が必要なファイル:
  - `src/features/auth/organisms/LoginPage.tsx` (Googleボタン追加)
  - `src/features/auth/organisms/SignUpPage.tsx` (Googleボタン追加)
  - `src/components/common/molecules/GoogleSignInButton.tsx` (新規作成)
  - `src/lib/api/endpoints.ts` (Google認証API追加)
  - `src/lib/api/hooks.ts` (Google認証フック追加)
- 環境変数:
  - `NEXT_PUBLIC_GOOGLE_CLIENT_ID`

**ラベル**: FrontEnd, Priority: High, Authentication

---

### 5. [BACK][FRONT] テストコードの実装基盤整備

**説明**:
現在、バックエンド・フロントエンドともにテストコードが存在しません。品質保証のため、テストフレームワークをセットアップし、基本的なテストコードを実装する必要があります。

**完了条件**:
- **バックエンド**:
  - xUnit + Moqがセットアップされている
  - Service層のUnitテストが少なくとも3つ以上実装されている
  - Repository層のIntegrationテストが少なくとも1つ以上実装されている
  - テストが`dotnet test`コマンドで実行できる
- **フロントエンド**:
  - Jest + React Testing Libraryがセットアップされている
  - コンポーネントのUnitテストが少なくとも3つ以上実装されている
  - テストが`npm test`コマンドで実行できる

**追加情報**:
- バックエンド実装が必要なファイル:
  - `nari-note-backend.Tests/nari-note-backend.Tests.csproj` (新規作成)
  - `nari-note-backend.Tests/Service/SignInServiceTests.cs` (新規作成)
  - `nari-note-backend.Tests/Service/CreateArticleServiceTests.cs` (新規作成)
  - `nari-note-backend.Tests/Repository/UserRepositoryTests.cs` (新規作成)
- フロントエンド実装が必要なファイル:
  - `jest.config.js` (新規作成)
  - `src/components/common/molecules/__tests__/EmailField.test.tsx` (新規作成)
  - `src/features/auth/organisms/__tests__/LoginPage.test.tsx` (新規作成)
- 必要なパッケージ:
  - バックエンド: `xunit`, `xunit.runner.visualstudio`, `Moq`, `Microsoft.EntityFrameworkCore.InMemory`
  - フロントエンド: `jest`, `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`

**ラベル**: BackEnd, FrontEnd, Priority: High, Testing

---

## 🟡 重要度：中（優先度を持って対応すべき）

### 6. [BACK] パスワードリセット機能のバックエンド実装

**説明**:
パスワードリセット機能を実装する必要があります。現在、ログインページに「パスワード忘れ」リンクは存在しますが、バックエンドの機能が未実装です。

**完了条件**:
- パスワードリセット申請APIエンドポイントが実装されている
- パスワードリセットトークンが生成・管理される
- メール送信機能が実装されている（パスワードリセットリンクを含む）
- パスワードリセット実行APIエンドポイントが実装されている
- トークンの有効期限が設定されている（例：24時間）

**追加情報**:
- 仕様書: `spec/signin.spec`（102-108行目）
- 実装が必要なファイル:
  - `Controller/AuthController.cs` (パスワードリセットエンドポイント追加)
  - `Application/Service/RequestPasswordResetService.cs` (新規作成)
  - `Application/Service/ResetPasswordService.cs` (新規作成)
  - `Application/Service/EmailService.cs` (新規作成)
  - `Domain/Entity/PasswordResetToken.cs` (新規作成)
  - `Domain/Repository/IPasswordResetTokenRepository.cs` (新規作成)
  - `Infrastructure/Repository/PasswordResetTokenRepository.cs` (新規作成)
  - `appsettings.json` (SMTP設定追加)
- 必要なNuGetパッケージ:
  - `MailKit` または `System.Net.Mail`
- 必要な環境変数:
  - `Smtp:Host`, `Smtp:Port`, `Smtp:Username`, `Smtp:Password`, `Smtp:From`

**ラベル**: BackEnd, Priority: Medium, Authentication

---

### 7. [FRONT] パスワードリセット機能のフロントエンド実装

**説明**:
パスワードリセット機能のフロントエンドUIを実装する必要があります。ログインページに「パスワード忘れ」リンクは存在しますが、リンク先のページが未実装です。

**完了条件**:
- パスワードリセット申請ページ（/password-reset）が実装されている
- メールアドレスを入力してリセット申請ができる
- パスワードリセット実行ページ（/password-reset/[token]）が実装されている
- 新しいパスワードを入力してリセットを実行できる
- 成功時・失敗時に適切なメッセージが表示される

**追加情報**:
- 仕様書: `spec/signin.spec`（102-108行目）
- 実装が必要なファイル:
  - `src/app/(auth)/password-reset/page.tsx` (新規作成)
  - `src/app/(auth)/password-reset/[token]/page.tsx` (新規作成)
  - `src/features/auth/organisms/PasswordResetRequestPage.tsx` (新規作成)
  - `src/features/auth/organisms/PasswordResetPage.tsx` (新規作成)
  - `src/lib/api/endpoints.ts` (パスワードリセットAPI追加)
  - `src/lib/api/hooks.ts` (パスワードリセットフック追加)

**ラベル**: FrontEnd, Priority: Medium, Authentication

---

### 8. [FRONT] 記事編集の自動保存機能実装

**説明**:
記事編集画面で一定時間が経過すると自動的に下書き保存される機能を実装する必要があります。現在、手動保存のみ実装されています。

**完了条件**:
- 記事編集中、一定時間（例：30秒）操作がないと自動的に下書き保存される
- 自動保存中であることを示すインジケーターが表示される
- 自動保存成功時に成功メッセージが表示される
- 自動保存失敗時にエラーメッセージが表示される
- 自動保存はdebounceされている（連続した変更時に過剰な保存を避ける）

**追加情報**:
- 仕様書: `spec/article-edit.spec`（87-92行目）
- 実装が必要なファイル:
  - `src/features/article/organisms/ArticleFormPage.tsx` (自動保存ロジック追加)
  - `src/lib/hooks/useAutosave.ts` (新規作成、カスタムフック)
- 使用するライブラリ:
  - `lodash.debounce` または useDebounceフックの自作

**ラベル**: FrontEnd, Priority: Medium

---

### 9. [BACK] 棋譜機能のバックエンド拡張

**説明**:
現在、盤面（BOD形式）の表示機能は実装されていますが、棋譜データを保存・管理する機能が未実装です。棋譜データを保存できるように、Articleエンティティを拡張する必要があります。

**完了条件**:
- Articleエンティティに棋譜データを保存するフィールドが追加されている
- 記事作成・更新APIで棋譜データを保存できる
- 記事取得APIで棋譜データを取得できる
- データベースマイグレーションが作成されている

**追加情報**:
- 仕様書: `spec/article.spec`（14-18行目）、`spec/article-edit.spec`（28行目）
- 実装が必要なファイル:
  - `Domain/Entity/Article.cs` (KifuDataフィールド追加)
  - `Migrations/` (新しいマイグレーション作成)
  - `Application/Dto/Request/CreateArticleRequest.cs` (KifuData追加)
  - `Application/Dto/Request/UpdateArticleRequest.cs` (KifuData追加)
  - `Application/Dto/Response/ArticleDetailResponse.cs` (KifuData追加)
- 棋譜データ形式の検討:
  - KIF形式、KIF+形式、またはJSON形式での保存

**ラベル**: BackEnd, Priority: Medium

---

### 10. [FRONT] 棋譜再生機能の実装

**説明**:
現在、盤面の静的表示は実装されていますが、棋譜の再生コントロール機能が未実装です。再生・一時停止・コマ送り等の機能を実装する必要があります。

**完了条件**:
- 記事エディタで棋譜フォーマット（KIF/KIF+）を挿入できる
- 記事表示ページで棋譜再生コントロールが表示される
- 再生ボタンで棋譜が自動再生される
- 一時停止ボタンで再生を停止できる
- 次へ/前へボタンでコマ送り/巻き戻しができる
- 特定の手番にジャンプできる

**追加情報**:
- 仕様書: `spec/article.spec`（14-18行目）、`spec/article-edit.spec`（74行目）
- 実装が必要なファイル:
  - `src/components/common/organisms/KifuPlayer.tsx` (新規作成)
  - `src/features/article/organisms/MarkdownEditor.tsx` (棋譜挿入機能追加)
  - `src/features/article/organisms/NarinoteMarkdown.tsx` (棋譜再生対応)
- 使用する可能性のあるライブラリ:
  - 棋譜パーサー（KIF/KIF+形式）の実装またはライブラリの選定
  - `next-shogi`（既に使用中）の拡張

**ラベル**: FrontEnd, Priority: Medium

---

### 11. [FRONT] Atomic Design構造の整理

**説明**:
現在、Atomic Designパターンが部分的に実装されていますが、構造が不完全です。`components/common/atoms/`ディレクトリが存在せず、Atoms/Moleculesの分類が明確でない部分があります。

**完了条件**:
- `components/common/atoms/`ディレクトリが作成されている
- 基本的なUI要素（Button, Input, Label等）がAtomsとして整理されている
- 機能単位のコンポーネント（FormField等）がMoleculesとして整理されている
- 実装ガイド（docs/implementation-guide.md）に沿った構造になっている
- 既存のコンポーネントが適切な階層に配置されている

**追加情報**:
- 参考ドキュメント: `nari-note-frontend/docs/implementation-guide.md`
- 実装が必要な作業:
  - `src/components/common/atoms/`ディレクトリの作成
  - 既存のコンポーネントの分類と移動
  - Atomsの実装（Button, Input, Label, Icon等）
  - Moleculesの見直しと整理

**ラベル**: FrontEnd, Priority: Medium, Refactor

---

### 12. [FRONT] エラーハンドリングUIの強化

**説明**:
バックエンドのエラーハンドリングは完全に実装されていますが、フロントエンドのエラーハンドリングUIが最小限です。ErrorBoundaryコンポーネントや統一的なエラー表示システムを実装する必要があります。

**完了条件**:
- ErrorBoundaryコンポーネントが実装されている
- 予期しないエラーが発生した場合、ErrorBoundaryでキャッチされる
- 共通エラーメッセージコンポーネントが実装されている
- API呼び出し時のエラーハンドリングが統一されている
- エラー表示のデザインガイドラインが確立されている

**追加情報**:
- 実装が必要なファイル:
  - `src/components/common/ErrorBoundary.tsx` (新規作成)
  - `src/components/common/ErrorMessage.tsx` (確認・改善)
  - `src/lib/utils/errorHandler.ts` (新規作成、エラーハンドリングユーティリティ)
  - `src/app/error.tsx` (Next.js エラーページ)
- 参考:
  - React公式ドキュメント: Error Boundaries

**ラベル**: FrontEnd, Priority: Medium

---

### 13. [BACK] Article.Bodyの最大長を修正

**説明**:
現在、`Article.Body`フィールドの最大長は10,000文字ですが、仕様書では50,000文字まで入力可能とされています。データベーススキーマとエンティティを修正する必要があります。

**完了条件**:
- `Article.Body`フィールドの`MaxLength`が50,000に変更されている
- データベースマイグレーションが作成・適用されている
- 既存データに影響がないことが確認されている

**追加情報**:
- 仕様書: `spec/article-edit.spec`（50,000文字まで入力可能）
- 現在の実装: `Domain/Entity/Article.cs`の`Body`プロパティが`MaxLength(10000)`
- 実装が必要なファイル:
  - `Domain/Entity/Article.cs` (MaxLength変更)
  - `Migrations/` (新しいマイグレーション作成)
- コマンド:
  ```bash
  dotnet ef migrations add UpdateArticleBodyMaxLength
  dotnet ef database update
  ```

**ラベル**: BackEnd, Priority: Medium, Bug

---

## 🟢 重要度：低（改善したい項目）

### 14. [FRONT] XSS対策の強化

**説明**:
基本的なセキュリティは実装されていますが、XSS（クロスサイトスクリプティング）対策を強化する必要があります。特に、リッチテキストエディタ（Tiptap）のサニタイズ設定を確認し、必要に応じてDOMPurifyを導入します。

**完了条件**:
- Tiptapのサニタイズ設定が確認・強化されている
- DOMPurifyライブラリが導入されている（必要に応じて）
- `dangerouslySetInnerHTML`の使用箇所が確認され、適切にサニタイズされている
- XSS攻撃のテストケースが作成されている

**追加情報**:
- 実装が必要なファイル:
  - `src/features/article/organisms/MarkdownEditor.tsx` (Tiptap設定確認)
  - `src/features/article/organisms/NarinoteMarkdown.tsx` (サニタイズ確認)
  - `src/lib/utils/sanitize.ts` (新規作成、サニタイズユーティリティ)
- 導入を検討するライブラリ:
  - `dompurify` + `@types/dompurify`
  - `isomorphic-dompurify`（サーバーサイドレンダリング対応）

**ラベル**: FrontEnd, Priority: Low, Security

---

### 15. [INFRA] CI/CDパイプラインの拡充

**説明**:
現在、Claude Code用のワークフローのみ存在します。ビルド・テストの自動化、Linterの自動実行、デプロイパイプラインを構築する必要があります。

**完了条件**:
- ビルドを自動化するGitHub Actionsワークフローが実装されている（バックエンド・フロントエンド）
- テストを自動化するGitHub Actionsワークフローが実装されている
- Linterを自動実行するGitHub Actionsワークフローが実装されている
- デプロイパイプラインが実装されている（Staging/Production環境）
- 依存関係の脆弱性スキャンが実装されている

**追加情報**:
- 実装が必要なファイル:
  - `.github/workflows/backend-ci.yml` (新規作成)
  - `.github/workflows/frontend-ci.yml` (新規作成)
  - `.github/workflows/deploy-staging.yml` (新規作成)
  - `.github/workflows/deploy-production.yml` (新規作成)
  - `.github/workflows/security-scan.yml` (新規作成)
- 使用するGitHub Actions:
  - `actions/checkout`
  - `actions/setup-dotnet`
  - `actions/setup-node`
  - `github/codeql-action`（セキュリティスキャン）

**ラベル**: Infrastructure, Priority: Low, CI/CD

---

### 16. [BACK][FRONT] パフォーマンス最適化

**説明**:
基本機能の実装は完了していますが、パフォーマンス最適化は未着手です。N+1クエリ問題、ページネーション、キャッシュ戦略、コードスプリッティング等を検討・実装する必要があります。

**完了条件**:
- **バックエンド**:
  - N+1クエリ問題が解消されている（Include/ThenIncludeの適切な使用）
  - ページネーションのパフォーマンスが最適化されている
  - キャッシュ戦略が検討・実装されている（Redis等）
- **フロントエンド**:
  - Lighthouseスコアが80点以上（Performance）
  - コードスプリッティングが適切に実装されている
  - 画像の最適化が実装されている（Next.js Image）
  - メモ化（useMemo, useCallback）が適切に使用されている

**追加情報**:
- バックエンド実装が必要な作業:
  - データベースクエリのパフォーマンス分析（Entity Framework Coreのログ確認）
  - Include/ThenIncludeの見直し
  - キャッシュ戦略の検討（Redis, メモリキャッシュ）
- フロントエンド実装が必要な作業:
  - Lighthouseスコアの測定
  - バンドルサイズの分析（`npm run build`後の分析）
  - コードスプリッティングの実装（dynamic import）
  - 画像の最適化（next/imageの使用）

**ラベル**: BackEnd, FrontEnd, Priority: Low, Performance

---

### 17. [FRONT] アクセシビリティ対応

**説明**:
Webアクセシビリティ（WCAG 2.1）に準拠するための実装が未着手です。WAI-ARIA属性、キーボード操作、スクリーンリーダー対応等を実装する必要があります。

**完了条件**:
- WAI-ARIA属性が適切に使用されている
- すべてのインタラクティブ要素がキーボードで操作できる
- スクリーンリーダーで適切に読み上げられる
- コントラスト比がWCAG 2.1 AA基準を満たしている
- フォーカス管理が適切に実装されている

**追加情報**:
- 実装が必要な作業:
  - ARIA属性の追加（role, aria-label, aria-describedby等）
  - キーボードナビゲーションの実装（Tab, Enter, Escape等）
  - フォーカストラップの実装（モーダル等）
  - コントラスト比の確認と修正
  - スクリーンリーダーのテスト（NVDA, JAWS等）
- 参考ツール:
  - axe DevTools（Chrome拡張）
  - Lighthouse（アクセシビリティスコア）

**ラベル**: FrontEnd, Priority: Low, Accessibility

---

### 18. [DOCS] API仕様書（OpenAPI/Swagger）の作成

**説明**:
バックエンドAPIの仕様書が存在しません。OpenAPI/Swagger仕様書を作成し、API仕様を明確にする必要があります。

**完了条件**:
- OpenAPI 3.0形式の仕様書が作成されている
- Swagger UIでAPIドキュメントが閲覧できる
- 全てのエンドポイントが文書化されている
- リクエスト/レスポンスの例が記載されている

**追加情報**:
- 実装が必要なファイル:
  - `nari-note-backend/swagger.json` または `swagger.yaml` (新規作成)
  - `Program.cs` (Swagger設定追加)
- 必要なNuGetパッケージ:
  - `Swashbuckle.AspNetCore`
- 参考:
  - OpenAPI Specification: https://swagger.io/specification/

**ラベル**: Documentation, Priority: Low

---

### 19. [DOCS] デプロイ手順書の作成

**説明**:
デプロイ手順が文書化されていません。Staging環境・Production環境へのデプロイ手順を明確にする必要があります。

**完了条件**:
- デプロイ手順書が作成されている
- Staging環境へのデプロイ手順が記載されている
- Production環境へのデプロイ手順が記載されている
- 環境変数の設定方法が記載されている
- ロールバック手順が記載されている

**追加情報**:
- 実装が必要なファイル:
  - `docs/deployment.md` (新規作成)
- 記載すべき内容:
  - 環境構成（Staging, Production）
  - デプロイ方法（Docker, Kubernetes, AWS, Azure等）
  - 環境変数の設定
  - データベースマイグレーション
  - ロールバック手順
  - トラブルシューティング

**ラベル**: Documentation, Priority: Low

---

### 20. [DOCS] トラブルシューティングガイドの作成

**説明**:
よくある問題とその解決方法を文書化したトラブルシューティングガイドが存在しません。開発者が問題に直面した際に参照できるガイドを作成する必要があります。

**完了条件**:
- トラブルシューティングガイドが作成されている
- よくある問題と解決方法が記載されている
- デバッグ方法が記載されている
- ログの確認方法が記載されている

**追加情報**:
- 実装が必要なファイル:
  - `docs/troubleshooting.md` (新規作成)
- 記載すべき内容:
  - Docker関連の問題
  - データベース接続の問題
  - 認証・認可の問題
  - API呼び出しの問題
  - ビルド・実行時の問題

**ラベル**: Documentation, Priority: Low

---

## まとめ

- **重要度：高** - 5件（通知、Google OAuth、テスト）
- **重要度：中** - 8件（パスワードリセット、自動保存、棋譜、Atomic Design、エラーハンドリング、Article.Body修正）
- **重要度：低** - 7件（XSS対策、CI/CD、パフォーマンス、アクセシビリティ、ドキュメント）
- **合計**: 20件

これらのissueを優先度順に対応していくことで、プロジェクトの完成度を高めることができます。
