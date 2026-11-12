# フロントエンド実装ガイド

ページ・機能を追加するときの手順と、コンポーネントの規約をまとめる。レイヤーの責務やレンダリング方針の背景は [architecture.md](./architecture.md)、API コードの扱いは [api-usage.md](./api-usage.md) を参照。

## 新しいページ・機能を追加する手順

1. **API を用意する**
   バックエンドに API を追加・変更したら、リポジトリのルートで `python scripts/api-generator.py` を実行して `lib/api` を再生成する。生成ファイルは手で編集しない。
2. **既存部品を探す**
   `components/ui`（Atom）、`components/molecules`、`components/organisms`、`features/global/organisms` に使えるものが無いか確認する。足りない汎用部品はここに追加する。
3. **Organism を作る**（機能固有の UI がある場合）
   `features/{feature}/organisms/` に置く。2 つ以上の機能で使うことになったら `components/organisms/` へ移す。
4. **Template を作る**
   `features/{feature}/templates/{Name}Template.tsx`。props で受けたデータとコールバックだけで描画し、レイアウトとレスポンシブ対応を担う。
5. **Page を作る**
   `features/{feature}/pages/{Name}Page.tsx`。`'use client'` を付け、生成フックでデータを取得し、ローディング・エラー分岐とイベントハンドラを書いて Template に渡す。ページ枠（`PageWithSidebar` など）もここで選ぶ。
6. **ルートを追加する**
   `app/(with-layout)/{route}/page.tsx` を作る。Server Component として `params` / `searchParams` を解決し、Page に渡す。
   - 公開ページ: `generateMetadata` で `lib/api/server.ts` の関数を呼び、`buildPageMetadata`（`lib/metadata.ts`）で metadata を返す。取得失敗時は `{}` を返す。
   - ログイン必須ページ: `metadata` に `robots: { index: false, follow: false }` を設定し、`AuthGuard redirectPath="..."` で包む。メール認証が必要なら内側を `VerifiedEmailGuard` で包む。
   - 検索エンジンに載せたい公開ページなら `app/sitemap.ts` への追加も検討する。
7. **再エクスポート**
   追加したコンポーネントを各ディレクトリの `index.ts` に追加する。
8. **確認**
   `npm run lint` と `npm run build` が通ることを確認する。

ルート側の最小例:

```tsx
// app/(with-layout)/users/[id]/page.tsx のような形（generateMetadata は省略）
export default async function UserProfilePageRoute({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return <UserProfilePage userId={id} />;
}
```

## コンポーネント規約

### 共通

- props はすべて `interface {Name}Props` で明示的に型を付ける。API のデータは `@/lib/api/types` の型をそのまま使う。
- `export function` の名前付きエクスポートにする（`app/` の `page.tsx` / `layout.tsx` のみ default export）。
- import はパスエイリアス `@/` を使う。
- `'use client'` は Page、フックや状態を使うコンポーネント、ブラウザ API を使うコンポーネントにだけ付ける。
- UI・コード・メッセージに絵文字を使わない（環境によって表示が崩れるため）。アイコンは `lucide-react` を使う。
- ブラウザの `alert` / `confirm` / `prompt` は使わない。確認は `DeleteConfirmModal` などのモーダル（`components/ui/dialog` ベース）で、結果の通知は `ErrorAlert` / `SuccessAlert` などの画面内表示で行う。
- 色はブランドトークンを使う（`globals.css` で定義。例: `bg-brand-primary`、`text-brand-text`、`border-brand-border`）。カラーコードを直接書かない。

### ローディング・エラー・空状態

Page での分岐には `components/ui` の共通コンポーネントを使い、独自の表示を作らない。

| 状態 | コンポーネント |
|---|---|
| 読み込み中 | `LoadingSpinner`（`text` で文言を指定） |
| 取得失敗 | `ErrorMessage`（`message`、再試行できるなら `onRetry={refetch}`） |
| データが空 | `EmptyState`（`title`、必要なら `description` / `action`） |
| フォーム送信エラー | `ErrorAlert` |

```tsx
if (isLoading) return <LoadingSpinner text="記事を読み込み中..." />;
if (error) return <ErrorMessage message="記事の取得に失敗しました" onRetry={refetch} />;
```

### 層ごとの注意

- Page は UI を持たない。Tailwind のクラスを書き始めたら Template に移す。
- Template と `components/` 配下の共通部品では API フックや `useAuth()` を呼ばず、必要な値は props で受け取る。
- 機能固有の Organism は、それだけで完結する操作（コメント投稿フォーム、画像アップロードなど）に限り Mutation フックや `useAuth()` を内部に持ってよい。ページ全体のデータ取得は Page に置く。ヘッダーなど `features/global/organisms` の全体部品も同様に認証状態を直接参照してよい。
- 認証が必要な操作は、ボタン押下時に `useRequireVerifiedEmail().ensureVerifiedEmail()` で事前に確認できる。401 / 403 は `client.ts` が共通モーダルを出すので、個別にハンドリングしない。

### 命名

| 種類 | 規則 | 例 |
|---|---|---|
| Page | `{Name}Page.tsx` | `ArticleDetailPage.tsx` |
| Template | `{Name}Template.tsx` | `ArticleDetailTemplate.tsx` |
| Organism / Molecule / 自作 Atom | PascalCase | `CommentList.tsx`, `TagChip.tsx` |
| shadcn/ui の Atom | kebab-case（CLI の生成どおり） | `button.tsx`, `dropdown-menu.tsx` |
| カスタムフック | `use{Name}.ts` | `useDebouncedValue.ts` |

## チェックリスト

- [ ] 既存の Atom / Molecule / Organism で代替できないか確認した
- [ ] 各層の責務を守っている（Page はロジックのみ、Template 以下は props のみ）
- [ ] props に型がある
- [ ] ローディング・エラー・空状態に共通コンポーネントを使っている
- [ ] `alert` / `confirm` と絵文字を使っていない
- [ ] 公開ページは metadata、ログイン必須ページは `AuthGuard` と `noindex` を設定した
- [ ] `index.ts` に再エクスポートした
- [ ] `npm run lint` / `npm run build` が通る
