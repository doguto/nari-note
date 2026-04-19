---
name: new-api-endpoint
description: nari-noteバックエンドに新しいAPIエンドポイントを追加するスキル。クリーンアーキテクチャパターン（Controller → Service → DTO → DI登録 → api-generator実行）に沿って実装する。ユーザーが「APIを作りたい」「エンドポイントを追加して」「〜APIを実装して」と言ったとき、または新しいバックエンドAPIの実装を求められたときは必ずこのスキルを使用する。
---

## 概要

このプロジェクト（nari-note）のバックエンドAPIを新規実装するスキルです。
以下のステップに沿って進めてください。

---

## ステップ1: 要件を明確にする

実装前に以下を確認する（不明な場合はユーザーに確認）：

- **目的**: このAPIは何をするのか
- **HTTPメソッド**: GET / POST / PUT / DELETE
- **エンドポイントパス**: どのControllerに追加するか
  - `AuthController` → 認証関連（`/api/auth/...`）
  - `UsersController` → ユーザー関連（`/api/users/...`）
  - `ArticlesController` → 記事関連（`/api/articles/...`）
  - など
- **認証要件**:
  - `[RequireAuth]` → ログイン必須
  - `[OptionalAuth]` → 任意（`UserId` が nullable）
  - `[AllowAnonymous]` → 誰でもアクセス可
- **リクエスト内容**: ボディ・パスパラメータ・クエリパラメータ
- **レスポンス内容**: 返すデータ（なければ空レスポンス）

---

## ステップ2: 既存パターンを参照する

類似のServiceやControllerを読んで、コードパターンを把握する。

```
nari-note-backend/src/Application/Service/   ← 類似Serviceを参照
nari-note-backend/src/Controller/            ← 追加先のControllerを読む
```

特に以下のパターンを確認しておく：
- ユーザー取得: `userRepository.FindForceByIdAsync(userId)`
- BCrypt: `BCrypt.Net.BCrypt.HashPassword()` / `BCrypt.Net.BCrypt.Verify()`
- 更新時: `user.UpdatedAt = DateTime.UtcNow;`
- エラー: `throw new ArgumentException("メッセージ")`

---

## ステップ3: Request DTOを作成する

`nari-note-backend/src/Application/Dto/Request/{ActionName}Request.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class {ActionName}Request
{
    [Required(ErrorMessage = "〜は必須です")]
    [MaxLength(255, ErrorMessage = "〜は255文字以内で入力してください")]
    public required string FieldName { get; set; }
}
```

> GETやDELETEでリクエストボディが不要な場合はスキップ可能。

---

## ステップ4: Response DTOを作成する

`nari-note-backend/src/Application/Dto/Response/{ActionName}Response.cs`

```csharp
namespace NariNoteBackend.Application.Dto.Response;

public class {ActionName}Response
{
    // 返すフィールドを定義する
    // データを返さない場合は空クラスでOK
}
```

---

## ステップ5: Serviceを作成する

`nari-note-backend/src/Application/Service/{ActionName}Service.cs`

```csharp
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class {ActionName}Service
{
    readonly IUserRepository userRepository;

    public {ActionName}Service(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<{ActionName}Response> ExecuteAsync(UserId userId, {ActionName}Request request)
    {
        // ビジネスロジックを実装する
        return new {ActionName}Response();
    }
}
```

> ユーザー認証不要のAPIは `UserId` パラメータを省略する。

---

## ステップ6: Controllerにエンドポイントを追加する

対象のControllerファイルに3箇所追加する：

### 6-1. フィールド宣言
```csharp
readonly {ActionName}Service {actionName}Service;
```

### 6-2. コンストラクタ引数と代入
```csharp
// 引数に追加
{ActionName}Service {actionName}Service,

// 代入に追加
this.{actionName}Service = {actionName}Service;
```

### 6-3. アクションメソッド
```csharp
[HttpPost("path")]
[RequireAuth]           // 認証要件に応じて変更
[ValidateModelState]    // リクエストボディがある場合
public async Task<ActionResult<{ActionName}Response>> {ActionName}([FromBody] {ActionName}Request request)
{
    var response = await {actionName}Service.ExecuteAsync(UserId!.Value, request);
    return Ok(response);
}
```

---

## ステップ7: DIに登録する

`nari-note-backend/src/Application/ApplicationServiceInstaller.cs`

```csharp
services.AddScoped<{ActionName}Service>();
```

---

## ステップ8: ビルドを確認する

```bash
cd /c/htogi/Scripts/nari-note/nari-note-backend && dotnet build 2>&1 | tail -10
```

エラー0件を確認する。

---

## ステップ9: api-generatorを実行する

フロントエンドのTypeScript型・エンドポイント・hooksを自動生成する：

```bash
cd /c/htogi/Scripts/nari-note && PYTHONIOENCODING=utf-8 python scripts/api-generator.py
```

新しいRequest/Response型とエンドポイントが `nari-note-frontend/src/lib/api/` に反映される。
