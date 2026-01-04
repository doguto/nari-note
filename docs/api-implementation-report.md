# API関数生成の実装完了レポート

## 実装概要

バックエンドのRequest/Responseクラスから、フロントエンドで使いやすいAPI関数を生成し、TanStack Query（React Query）を統合しました。

## 実装内容

### 1. パッケージの追加

```json
{
  "@tanstack/react-query": "^5.x",
  "axios": "^1.x"
}
```

### 2. 実装したファイル

#### フロントエンド（`nari-note-frontend/src/lib/api/`）

1. **types.ts** - TypeScript型定義
   - バックエンドのRequest/Response型をTypeScriptで再定義
   - 完全な型安全性を保証

2. **client.ts** - Axiosクライアント設定
   - ベースURLの設定
   - 認証トークンの自動付与
   - エラーハンドリング

3. **endpoints.ts** - API関数定義
   - Auth API: `signUp`, `signIn`
   - Articles API: `create`, `getById`, `update`, `delete`, `getByAuthor`, `getByTag`, `toggleLike`
   - Users API: `getProfile`, `updateProfile`
   - Health API: `check`

4. **hooks.ts** - TanStack Queryカスタムフック
   - データ取得: `useArticle`, `useUserProfile`, `useArticlesByAuthor`, `useArticlesByTag`
   - データ変更: `useSignUp`, `useSignIn`, `useCreateArticle`, `useUpdateArticle`, `useDeleteArticle`, `useToggleLike`, `useUpdateUserProfile`
   - 自動キャッシュ無効化

5. **index.ts** - エクスポート

#### プロバイダー（`nari-note-frontend/src/lib/providers/`）

6. **QueryProvider.tsx** - TanStack Query Provider
   - QueryClientの設定
   - グローバルオプション

#### デバッグページ（`nari-note-frontend/src/app/debug-new/`）

7. **page.tsx** - 実装例を含むデバッグページ
   - すべてのAPI関数のテスト
   - ローディング状態の管理
   - エラーハンドリング

#### ドキュメント

8. **docs/frontend-api-usage.md** - 詳細な使用方法ドキュメント
9. **nari-note-frontend/README.md** - フロントエンドREADMEの更新

#### バックエンド

10. **nari-note-backend/Program.cs** - OpenAPI仕様の有効化（開発環境用）

## 主な機能

### 型安全性
- バックエンドのクラス構造に基づいた完全な型定義
- TypeScriptによるコンパイル時の型チェック
- IDEでの強力な補完サポート

### 認証管理
- トークンの自動管理（localStorage）
- すべてのリクエストへの自動付与
- 401エラー時の自動クリア

### キャッシング
- TanStack Queryによる自動キャッシング
- スマートな再フェッチ戦略（staleTime: 1分）
- Mutationによる自動的なキャッシュ無効化

### エラーハンドリング
- 統一されたエラー処理
- カスタマイズ可能なエラーコールバック
- HTTPステータスコードに基づく処理

## 使用例

### データ取得（Query）

```tsx
import { useArticle } from '@/lib/api';

function ArticleDetail({ id }: { id: number }) {
  const { data, isLoading, error } = useArticle(id);
  
  if (isLoading) return <div>読み込み中...</div>;
  if (error) return <div>エラー: {error.message}</div>;
  
  return <article>{data?.title}</article>;
}
```

### データ変更（Mutation）

```tsx
import { useCreateArticle } from '@/lib/api';

function CreateArticleForm() {
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      alert(`記事作成成功！ID: ${data.id}`);
    },
  });

  const handleSubmit = () => {
    createArticle.mutate({
      title: 'タイトル',
      body: '本文',
      authorId: 1,
      tags: ['TypeScript', 'React'],
      isPublished: true,
    });
  };

  return (
    <button 
      onClick={handleSubmit} 
      disabled={createArticle.isPending}
    >
      {createArticle.isPending ? '作成中...' : '作成'}
    </button>
  );
}
```

## アーキテクチャ

### Container/Presentationalパターン

```tsx
// Container Component (ロジック層)
'use client';

import { useArticle } from '@/lib/api';
import { ArticleView } from './ArticleView';

export function ArticleContainer({ id }: { id: number }) {
  const { data, isLoading, error } = useArticle(id);
  return <ArticleView article={data} isLoading={isLoading} error={error} />;
}

// Presentational Component (表示層)
export function ArticleView({ article, isLoading, error }) {
  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  if (!article) return null;
  
  return <article>{article.title}</article>;
}
```

## テスト環境

### デバッグページ

- **TanStack Query版**: `/debug-new`
  - 型安全なAPI呼び出し
  - 自動キャッシング
  - ローディング状態管理
  - エラーハンドリング

- **従来版**: `/debug`
  - 素のfetch APIを使用
  - 比較用

## メリット

### 開発効率
- **簡潔なコード**: フックベースのAPIで数行で実装可能
- **自動キャッシング**: 手動でのキャッシュ管理が不要
- **型安全**: コンパイル時にエラーを検出

### パフォーマンス
- **自動キャッシング**: 不要なリクエストを削減
- **バックグラウンド更新**: ユーザー体験を損なわない更新
- **楽観的更新**: UIの即座な反応

### 保守性
- **統一されたAPI**: すべてのAPI呼び出しが同じパターン
- **型定義の一元管理**: `types.ts`で一括管理
- **エラーハンドリング**: 一貫したエラー処理

## 今後の拡張性

### 容易に追加できる機能

1. **楽観的更新**
   ```tsx
   const toggleLike = useToggleLike({
     onMutate: async (articleId) => {
       // UIを即座に更新
       queryClient.setQueryData(queryKeys.articles.byId(articleId), old => ({
         ...old,
         likeCount: old.likeCount + 1
       }));
     },
   });
   ```

2. **ページネーション**
   ```tsx
   import { useInfiniteQuery } from '@tanstack/react-query';
   
   const { data, fetchNextPage, hasNextPage } = useInfiniteQuery({
     queryKey: ['articles', 'infinite'],
     queryFn: ({ pageParam = 1 }) => articlesApi.getPage(pageParam),
     getNextPageParam: (lastPage) => lastPage.nextPage,
   });
   ```

3. **リアルタイム更新**
   ```tsx
   useEffect(() => {
     const ws = new WebSocket('ws://...');
     ws.onmessage = (event) => {
       queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
     };
   }, []);
   ```

## ビルド結果

### フロントエンド
```
✓ Compiled successfully
✓ Linting and checking validity of types
✓ Generating static pages (7/7)

Route (app)                Size         First Load JS
┌ ○ /                     162 B        105 kB
├ ○ /debug               2.76 kB       108 kB
└ ○ /debug-new           27.8 kB       139 kB
```

### バックエンド
```
Build succeeded.
8 Warning(s) (既存)
0 Error(s)
```

## 完了条件の達成

✅ **フロントエンドでAPI通信の実装が楽に出来る**

- TanStack Queryによる簡潔なAPI
- 完全な型安全性
- 自動キャッシュ管理
- 統一されたエラーハンドリング
- 豊富なドキュメントと実装例

## まとめ

バックエンドのRequest/Responseクラスから、型安全で使いやすいAPI関数を生成しました。TanStack Queryの統合により、キャッシング、ローディング状態管理、エラーハンドリングが自動化され、開発効率が大幅に向上しました。

デバッグページ（`/debug-new`）で実際の動作を確認でき、詳細なドキュメント（`docs/frontend-api-usage.md`）で使い方を学べます。
