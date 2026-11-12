# API の使い方

フロントエンドからバックエンド API を呼ぶためのコードは、バックエンドの C# コードから `scripts/api-generator.py` で生成する。通信経路やキャッシュ方針の背景は [architecture.md](./architecture.md) を参照。

## lib/api の構成

| ファイル | 生成 | 役割 |
|---|---|---|
| `types.ts` | 生成 | Request / Response / DTO の TypeScript 型 |
| `endpoints.ts` | 生成 | Controller ごとの API 関数（`articlesApi.getArticleContent` など）。`client.ts` を使い BFF プロキシ経由で呼ぶ |
| `hooks.ts` | 生成 | TanStack Query のフック（`useGetArticleContent`、`useCreateArticle` など）と `queryKeys` |
| `server.ts` | 生成 | Server Component / Route Handler 用の GET 関数。`API_URL` に直接 fetch する |
| `client.ts` | 手書き | axios インスタンス、401 / 403 の共通ハンドリング |
| `index.ts` | 手書き | 再エクスポート |

生成ファイルは手で編集しない（次回の生成で上書きされる）。API 一覧の正はバックエンドの Controller（または Swagger）で、生成物はその写しである。

## 生成のしかた

バックエンドの Controller や DTO を追加・変更したら、リポジトリのルートで実行する（パスはルートからの相対で解決される）。

```bash
python scripts/api-generator.py           # 生成
python scripts/api-generator.py --verbose # スキップしたメソッドの理由も表示
```

生成後は `git diff nari-note-frontend/src/lib/api` で差分を確認し、`npm run build` で型エラーが無いことを確かめる。

### 生成されるための前提（バックエンド側の書き方）

ジェネレーターは C# を正規表現で解析するため、次の形に揃える。

- Controller は `public class XxxController`。パスは `/api/{xxx を小文字にしたもの}/{HttpXxx 属性のルート}` になる。
- アクションは `[HttpGet]` / `[HttpPost]` / `[HttpPut]` / `[HttpDelete]` を付け、戻り値を `ActionResult<XxxResponse>`（`async Task<...>` 可）にする。
- DTO は `Application/Dto/Request` / `Response`（および `Application/Dto` 直下）に置き、メソッド名に対応させて `{メソッド名}Request` / `{メソッド名}Response` と命名する。Response 型が特定できないメソッドは生成対象から外れる（DELETE と Logout は `void` 扱い）。
- ボディは `[FromBody] XxxRequest`、ファイルは `IFormFile` 引数で受ける。単純型の `[FromQuery]` 引数はクエリパラメータになる。
- ID などの ValueObject は `Domain/ValueObject/EntityKeyObject.cs` の定義を読んで型を決める。

詳しい手順はスキル `.agents/skills/api-generator/SKILL.md` を参照。

### 生成されるものの規則

- 関数名はメソッド名の camelCase、フック名は `use{メソッド名}`。
- GET は `useQuery`、それ以外は `useMutation` のフックになる。
- GET の引数は、パスパラメータがあれば URL に埋め込み、無ければ Request 全体をクエリパラメータとして送る。
- パスパラメータに対応する Request のフィールドは optional になる。
- `server.ts` は GET のみ生成される。

## 使い方

### データ取得（Page で使う）

```tsx
const { data, isLoading, error, refetch } = useGetArticleContent({ id: articleId });
// useQuery のオプションは最後の引数で渡す（パラメータ無しのフックでは第 1 引数）
const { data: mine } = useGetMyArticles({ enabled: !!userId });
```

### 更新

```tsx
const createComment = useCreateComment({
  onSuccess: () => setMessage(''),
  onError: (e) => setError(e.message),
});
createComment.mutate({ articleId, message });
```

- 成功時には同じ Controller のクエリが自動で invalidate されるので、`onSuccess` で `refetch()` を呼ぶ必要はない（[キャッシュ方針](./architecture.md#tanstack-query-のキャッシュ方針)）。
- エラーの `message` にはバックエンドが返したメッセージが入るので、そのまま画面に表示してよい。
- 401 / 403 は `client.ts` が共通モーダルを出すため、個別に処理しなくてよい。

### サーバー側での取得

`app/` の `generateMetadata`、`sitemap.ts`、Route Handler では `@/lib/api/server` の関数を使う。クライアント用の `endpoints.ts` / `hooks.ts` はサーバーでは使わない。

```tsx
const { article } = await getArticleContent({ id });
```

`server.ts` は Cookie を送らないため、ログイン不要の GET にだけ使う。

## 手書きしてよい範囲

- `client.ts` / `index.ts`
- 生成フックを組み合わせたカスタムフック（`lib/hooks/` または `features/{feature}/` 配下）
- 生成フックに渡すオプション（`enabled`、`onSuccess` など）や、別 Controller のクエリの追加 invalidate
- バックエンドを経由しない Route Handler（`app/api/og` など）とそのための型

生成結果に不足や誤りがあるときは、生成ファイルを直さず `scripts/api-generator.py` / `scripts/templates/` かバックエンド側の書き方を直して再生成する。
