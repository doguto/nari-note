# Nari-Note フロントエンド

技術記事の投稿・共有プラットフォーム「なりノート」のフロントエンド実装です。

## 技術スタック

- **フレームワーク**: Next.js 15
- **言語**: TypeScript
- **スタイリング**: TailwindCSS
- **データフェッチング**: TanStack Query (React Query)
- **HTTPクライアント**: Axios
- **アーキテクチャ**: Container/Presentationalパターン

## Getting Started

### 開発サーバーの起動

```bash
npm install
npm run dev
```

[http://localhost:3000](http://localhost:3000)にアクセスしてアプリケーションを確認できます。

### ビルド

```bash
npm run build
npm start
```

### Linter

```bash
npm run lint
```

## API通信

このプロジェクトでは、TanStack QueryとAxiosを使用してバックエンドAPIと通信します。

### 主な機能

- **型安全**: TypeScriptによる完全な型定義
- **自動キャッシング**: スマートなデータキャッシング戦略
- **認証管理**: 自動的なトークン管理
- **エラーハンドリング**: 統一されたエラー処理

### 使用例

```tsx
import { useArticle, useCreateArticle } from '@/lib/api';

// データ取得
function ArticleDetail({ id }: { id: number }) {
  const { data, isLoading, error } = useArticle(id);
  
  if (isLoading) return <div>読み込み中...</div>;
  if (error) return <div>エラー: {error.message}</div>;
  
  return <article>{data.title}</article>;
}

// データ作成
function CreateArticleForm() {
  const createArticle = useCreateArticle({
    onSuccess: (data) => alert(`作成成功！ID: ${data.id}`),
  });

  const handleSubmit = () => {
    createArticle.mutate({
      title: 'タイトル',
      body: '本文',
      authorId: 1,
      tags: ['TypeScript'],
    });
  };

  return (
    <button onClick={handleSubmit} disabled={createArticle.isPending}>
      作成
    </button>
  );
}
```

詳細な使い方は[API使用ガイド](./docs/frontend-api-usage.md)を参照してください。

## ディレクトリ構造

```
src/
├── app/                    # Next.js App Router
│   ├── page.tsx           # トップページ
│   ├── debug/             # デバッグページ（従来のfetch API）
│   └── debug-new/         # デバッグページ（TanStack Query版）
└── lib/
    ├── api/               # API関連
    │   ├── client.ts      # Axiosクライアント設定
    │   ├── types.ts       # 型定義
    │   ├── endpoints.ts   # API関数
    │   ├── hooks.ts       # TanStack Queryフック
    │   └── index.ts       # エクスポート
    └── providers/         # Providerコンポーネント
        └── QueryProvider.tsx
```

## 環境変数

`.env.local`ファイルを作成して、以下の環境変数を設定してください：

```bash
# APIエンドポイント
NEXT_PUBLIC_API_URL=http://localhost:5243
```

## デバッグツール

### TanStack Query版デバッグページ

[http://localhost:3000/debug-new](http://localhost:3000/debug-new)

- TanStack Queryを使用した実装例
- 型安全なAPI呼び出し
- ローディング状態とエラーハンドリングのデモ

### 従来版デバッグページ

[http://localhost:3000/debug](http://localhost:3000/debug)

- 素のfetch APIを使用した実装

## ドキュメント

- [API使用ガイド](./docs/frontend-api-usage.md) - API関数の詳細な使い方
- [フロントエンド実装ガイド](./docs/frontend-implementation-guide.md) - Atomic Designとコンポーネント生成パターン
- [フロントエンドアーキテクチャ](./docs/frontend-architecture.md) - アーキテクチャの詳細ガイド
- [クイックリファレンス](./docs/frontend-quick-reference.md) - よく使うパターン集

## Learn More

Next.jsについて詳しく知りたい方は以下のリソースを参照してください：

- [Next.js Documentation](https://nextjs.org/docs)
- [TanStack Query Documentation](https://tanstack.com/query/latest)
- [Axios Documentation](https://axios-http.com/)

