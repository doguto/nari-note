---
name: new-api-endpoint
description: nari-noteバックエンドに新しいAPIエンドポイントを追加するスキル。レイヤードアーキテクチャ（DTO → Service → Controller → DI登録 → テスト → api-generator実行）に沿って実装する。ユーザーが「APIを作りたい」「エンドポイントを追加して」「〜APIを実装して」と言ったとき、または新しいバックエンドAPIの実装を求められたときは必ずこのスキルを使用する。
---

## 概要

nari-note のバックエンド API を新規実装する手順。規約（`this.` の使用、`private` 省略、XML ドキュメントコメント禁止、`TimeProvider`、例外の使い分け）はルートの `AGENTS.md`、詳しい実装パターンは `nari-note-backend/Documents/backend-implementation-guide.md` を正とする。

パスはすべて `nari-note-backend/` からの相対パス。

---

## ステップ1: 要件を明確にする

不明な点はユーザーに確認する。

- **目的**: この API は何をするのか
- **HTTP メソッドとパス**: どの Controller（`Src/Controller/`）に追加するか。ルートは `api/[controller]`（例: `ArticlesController` → `/api/articles/...`）
- **認証要件**（`Src/Filter/AuthLevel.cs`）:
  - `[Auth(AuthLevel.VerifiedEmail)]` → ログイン必須 + メール認証済み
  - `[Auth(AuthLevel.Authenticated)]` → ログイン必須
  - `[Auth(AuthLevel.Optional)]` → 任意（`UserId` が null になりうる）
  - `[Auth(AuthLevel.Anonymous)]` → 誰でもアクセス可
- **リクエスト**: ボディ・パスパラメータ・クエリパラメータ
- **レスポンス**: 返すデータ（無ければ空の Response）
- **DB 変更の有無**: Entity / カラムの追加が必要か

---

## ステップ2: 既存パターンを読む

類似の Service / Controller / Repository を読み、パターンを揃える。

- `Src/Application/Service/` - 類似 Service
- `Src/Controller/` - 追加先の Controller
- `Src/Domain/Repository/`（インターフェース）、`Src/Infrastructure/Repository/`（実装）

よく使うパターン:

- 取得: `this.userRepository.FindForceByIdAsync(userId)`（見つからなければ `KeyNotFoundException` → 404）。null を扱いたい場合は `FindByIdAsync`
- 現在時刻: `this.timeProvider.UtcNow()`（`TimeProvider` をコンストラクタで受け取る。`NariNoteBackend.Extension` の拡張メソッド）
- 入力不正: `throw new ArgumentException("メッセージ")` → 400
- 権限不足 / 競合: `ForbiddenException` / `ConflictException`（`Src/Application/Exception/`）

---

## ステップ3: DB 変更がある場合

1. `Src/Domain/Entity/` に Entity を追加・変更する
2. マイグレーションを追加する

```bash
cd nari-note-backend
dotnet ef migrations add <マイグレーション名>
```

マイグレーションはアプリ起動時に自動適用される。手順の詳細は `Documents/development-workflow.md`。

---

## ステップ4: Repository を追加・拡張する（必要な場合）

- インターフェース: `Src/Domain/Repository/I{Name}Repository.cs`
- 実装: `Src/Infrastructure/Repository/{Name}Repository.cs`
- 新しい Repository は `Src/Infrastructure/InfrastructureServiceInstaller.cs` に登録する

```csharp
services.AddScoped<I{Name}Repository, {Name}Repository>();
```

N+1 にならないよう、必要な関連は `Include` 等でまとめて取得する。

---

## ステップ5: Request DTO を作成する

`Src/Application/Dto/Request/{ActionName}Request.cs`

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

ボディもパラメータも無い場合は省略してよい。

---

## ステップ6: Response DTO を作成する

`Src/Application/Dto/Response/{ActionName}Response.cs`

```csharp
namespace NariNoteBackend.Application.Dto.Response;

public class {ActionName}Response
{
}
```

返すデータが無い場合も空クラスを作る。名前を `{Controller のアクション名}Response` にしておくと、api-generator がレスポンス型を推測できる。

---

## ステップ7: Service を作成する

`Src/Application/Service/{ActionName}Service.cs`（API 一個につき Service 一個）

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
        return new {ActionName}Response();
    }
}
```

- 認証不要の API では `UserId` 引数を省略する
- try-catch はしない（`GlobalExceptionHandlerMiddleware` が変換する）
- トランザクションは `TransactionMiddleware` が管理するので Service では張らない

---

## ステップ8: Controller にアクションを追加する

対象 Controller（`ApplicationController` を継承）に、フィールド・コンストラクタ引数・アクションを追加する。

```csharp
readonly {ActionName}Service {actionName}Service;

// コンストラクタ
this.{actionName}Service = {actionName}Service;

[HttpPost("path")]
[Auth(AuthLevel.Authenticated)]
[ValidateModelState]
public async Task<ActionResult<{ActionName}Response>> {ActionName}([FromBody] {ActionName}Request request)
{
    var response = await this.{actionName}Service.ExecuteAsync(UserId!.Value, request);
    return Ok(response);
}
```

- `[ValidateModelState]` はリクエストボディがある場合に付ける
- ファイルアップロード等でトランザクションを張りたくない場合は `[NoTransaction]`

---

## ステップ9: DI に登録する

`Src/Application/ApplicationServiceInstaller.cs`

```csharp
services.AddScoped<{ActionName}Service>();
```

---

## ステップ10: テストを追加し、ビルド・テストする

テストの方針と配置は `Documents/testing-guide.md` に従う（`Tests/NariNoteBackend.Tests/` 配下）。

```bash
cd nari-note-backend
dotnet build nari-note-backend.sln
dotnet test nari-note-backend.sln
```

結合テストは Testcontainers で PostgreSQL を起動するため Docker が必要。

---

## ステップ11: フロントエンドの API コードを生成する

リポジトリルートで実行する。

```bash
PYTHONIOENCODING=utf-8 python scripts/api-generator.py --verbose
```

`nari-note-frontend/src/lib/api/` の `types.ts` / `endpoints.ts` / `hooks.ts` / `server.ts` が再生成される。`Skipped methods` に今回のアクションが出た場合は、戻り値を `ActionResult<{ActionName}Response>` にする。詳細は `.agents/skills/api-generator/SKILL.md`。
