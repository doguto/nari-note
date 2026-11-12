# バックエンド実装ガイド

バックエンドのコーディング規約と、層ごとの実装ルールをまとめる。層の構成と例外→HTTP ステータスの対応は [architecture.md](./architecture.md) を参照。

## コーディング規約

### フィールドと `this.`

- private フィールドは `_` を付けないキャメルケース。`readonly` を付ける
- クラスフィールドの `private` 修飾子は省略する（`public` / `protected` / `internal` は明記する）
- private フィールドへのアクセスには `this.` を付ける

```csharp
public class GetArticleContentService
{
    readonly IArticleRepository articleRepository;

    public GetArticleContentService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
}
```

既存コードには `this.` の無いアクセスも残っているが、新規・修正箇所では規約に従う。

### コメント

- XML ドキュメントコメント（`/// <summary>` 等）は使わず、`//` で書く
- コードを読めば分かることは書かない。書くのは「なぜそうしているか」（制約、セキュリティ上の理由、意図的な非対称など）
- ログメッセージは英語のシステム的な文言にし、値は構造化ログのプレースホルダで渡す（例: `"Outbox message failed. Id={Id}"`）

### 日付時刻

- 現在時刻は DI した `TimeProvider` から `timeProvider.UtcNow()` で取得する。`DateTime.UtcNow` / `DateTime.Now` は使わない（テストで時刻を固定できなくなるため）
- Entity は自分で現在時刻を取得しない。判定に時刻が必要なら引数で受け取る（例: `article.IsPubliclyVisibleAt(now)`）
- `CreatedAt` / `UpdatedAt` は未設定なら `NariNoteDbContext` が保存時に設定する。更新時の `UpdatedAt` は Service で明示的に設定する

### ID の ValueObject（Vogen）

- Entity の ID は Vogen の ValueObject を使う（`UserId`, `ArticleId` 等。`Src/Domain/ValueObject/EntityKeyObject.cs`）。`int` / `Guid` を直接扱わず、型で取り違えを防ぐ
- 新しい Entity を追加したら、ValueObject の定義に加えて、UUID の場合は `Src/Infrastructure/Database/IdValueGenerator/` に採番クラス（UUID v7）を、JSON で返す場合は `Src/Infrastructure/Binding/ValueObjectJsonConverterFactory.cs` への登録を追加する
- int の ID は DB の IDENTITY 列で採番する

### 例外の使い分け

Controller / Service / Repository で try-catch はしない。状況に合った例外を throw すれば、グローバル例外ハンドラーが HTTP ステータスに変換する。

| 状況 | 例外 |
|---|---|
| 入力不正、ビジネスルール違反、無効なトークン | `ArgumentException` |
| リソースが存在しない | `KeyNotFoundException`（通常は Repository の `FindForce*Async` に任せる） |
| 他人のリソースを操作しようとした | `ForbiddenException` |
| 重複・状態の競合 | `ConflictException` |
| 送信間隔などの回数制限 | `TooManyRequestsException` |

- 例外メッセージは（500 以外）そのままクライアントに返るため、ユーザーに見せる日本語で書く
- 存在を隠したいリソース（他人の下書き等）は 403 ではなく 404 にする

## 層ごとのルール

### Controller

- `ApplicationController` を継承し、認証済みユーザーは `UserId` / `UserName` / `IsEmailVerified` で参照する
- 各アクションに `[Auth(AuthLevel.X)]` を付ける（レベルの意味は [authentication-strategy.md](./authentication-strategy.md#認証レベル)）。リクエストボディを受け取るアクションには `[ValidateModelState]` を付ける
- 1 アクションで呼ぶ Service は 1 つ。ビジネスロジックは書かない
- パスパラメータの ID は ValueObject 型（`ArticleId` 等）で受け取る
- 外部ストレージへの書き込み等でリクエスト全体のトランザクションを張りたくない場合は `[NoTransaction]` を付ける

### Service

- **API 一個につき Service 一個**。クラス名は `{動詞}{対象}Service`、公開メソッドは `ExecuteAsync` 1 つ
- 入力は Request DTO（必要に応じて `UserId` 等の引数を追加）、出力は Response DTO
- Repository / Gateway の interface に依存する。DbContext や外部 SDK を直接使わない
- メール送信・Discord 通知は直接呼ばず、`OutboxMessage` を `IOutboxMessageRepository` に追加する（[architecture.md](./architecture.md#outbox-によるメールdiscord-通知の配送)）
- トランザクションはミドルウェアが張るので、Service では管理しない。外部ストレージの削除など DB でロールバックできない処理は、DB 操作の後に置く

### Repository

- interface は `Src/Domain/Repository/`、実装は `Src/Infrastructure/Repository/`。共通操作は `IRepository<T, TId>` を継承する
- `Find*Async` は見つからなければ `null`、`FindForce*Async` は `KeyNotFoundException` を throw
- 必要なナビゲーションプロパティは明示的に `Include` する。一覧表示など重い列が不要な場合は `Src/Domain/ReadModel/` の型に射影して取得する
- キャッシュ等のインフラ最適化はここで吸収し、Service に漏らさない

### Entity

- `Src/Domain/Entity/` に置き、`EntityBase` を継承する
- 制約は Data Annotations（`[Key]`, `[Required]`, `[MaxLength]`, `[Index]` 等）で書き、リレーションと削除時の挙動は `NariNoteDbContext.OnModelCreating` で設定する
- 非 null のプロパティには `required` を付ける
- 単純なドメインロジック（公開状態の判定、いいね済み判定等）は Entity のメソッドに置く

### DTO

- Request は `Src/Application/Dto/Request/`、Response は `Src/Application/Dto/Response/`。名前は `{Service 名から Service を除いたもの}Request / Response`
- Request には DataAnnotations の検証属性と日本語のエラーメッセージを付ける
- JSON は camelCase で出力される。DTO はフロントエンドの API コード生成（`scripts/api-generator.py`）の入力になるため、変更したら再生成する

## 新規 API の追加手順

詳細な手順とテンプレートはスキル [.agents/skills/new-api-endpoint/SKILL.md](../../.agents/skills/new-api-endpoint/SKILL.md) を参照。流れは次のとおり。

1. 要件（HTTP メソッド・パス・認証レベル・入出力）を決める
2. DB 変更が必要なら Entity を追加・変更し、マイグレーションを作成する（[development-workflow.md](./development-workflow.md#マイグレーション)）
3. 必要なら Repository の interface と実装にメソッドを追加する（新しい Repository は `InfrastructureServiceInstaller` に登録）
4. Request / Response DTO を作成する
5. Service を作成し、`ApplicationServiceInstaller` に登録する
6. Controller にアクションを追加する
7. Service の単体テストと Controller の結合テストを書く（[testing-guide.md](./testing-guide.md)）
8. `dotnet build` と `dotnet test` が通ることを確認する
9. リポジトリルートで `python scripts/api-generator.py` を実行し、フロントエンドの API コードを再生成する
