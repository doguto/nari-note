# API関数の使用方法

このドキュメントでは、バックエンドAPIとの通信に使用するAPI関数の使い方を説明します。

## 概要

フロントエンドでは、TanStack Query（React Query）とAxiosを使用してAPIとの通信を行います。
すべてのAPI関数は型安全で、自動的にキャッシュとリフェッチを管理します。

## セットアップ

プロジェクトにはすでにQueryProviderが設定されているため、追加のセットアップは不要です。

### 環境変数

API URLを設定するには、`.env.local`ファイルを作成します：

```bash
NEXT_PUBLIC_API_URL=http://localhost:5243
```

デフォルトでは`http://localhost:5243`が使用されます。

## 基本的な使い方

### 1. 認証

#### サインアップ

```tsx
'use client';

import { useSignUp } from '@/lib/api';

export function SignUpForm() {
  const signUp = useSignUp({
    onSuccess: (data) => {
      console.log('サインアップ成功！', data);
      // トークンは自動的にlocalStorageに保存されます
    },
    onError: (error) => {
      console.error('サインアップエラー:', error);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    signUp.mutate({
      email: 'test@example.com',
      name: 'testuser',
      password: 'Password123!',
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* フォームフィールド */}
      <button type="submit" disabled={signUp.isPending}>
        {signUp.isPending ? '処理中...' : 'サインアップ'}
      </button>
    </form>
  );
}
```

#### サインイン

```tsx
import { useSignIn } from '@/lib/api';

const signIn = useSignIn();

signIn.mutate({
  usernameOrEmail: 'test@example.com',
  password: 'Password123!',
});
```

### 2. 記事操作

#### 記事の作成

```tsx
import { useCreateArticle } from '@/lib/api';

const createArticle = useCreateArticle({
  onSuccess: (data) => {
    console.log('記事作成成功！ID:', data.id);
  },
});

createArticle.mutate({
  title: '新しい記事',
  body: '記事の内容',
  authorId: 1,
  tags: ['TypeScript', 'React'],
  isPublished: true,
});
```

#### 記事の取得

```tsx
import { useArticle } from '@/lib/api';

export function ArticleDetail({ articleId }: { articleId: number }) {
  const { data, isLoading, error } = useArticle(articleId);

  if (isLoading) return <div>読み込み中...</div>;
  if (error) return <div>エラー: {error.message}</div>;
  if (!data) return null;

  return (
    <article>
      <h1>{data.title}</h1>
      <p>著者: {data.authorName}</p>
      <div>{data.body}</div>
      <div>いいね: {data.likeCount}</div>
    </article>
  );
}
```

#### 記事の更新

```tsx
import { useUpdateArticle } from '@/lib/api';

const updateArticle = useUpdateArticle();

updateArticle.mutate({
  id: 1,
  data: {
    title: '更新されたタイトル',
    body: '更新された内容',
    tags: ['Updated'],
  },
});
```

#### 記事の削除

```tsx
import { useDeleteArticle } from '@/lib/api';

const deleteArticle = useDeleteArticle();

deleteArticle.mutate({
  id: 1,
  userId: 1,
});
```

#### 著者別の記事取得

```tsx
import { useArticlesByAuthor } from '@/lib/api';

export function AuthorArticles({ authorId }: { authorId: number }) {
  const { data, isLoading } = useArticlesByAuthor(authorId);

  if (isLoading) return <div>読み込み中...</div>;

  return (
    <div>
      {data?.articles.map((article) => (
        <div key={article.id}>{article.title}</div>
      ))}
    </div>
  );
}
```

#### タグ別の記事取得

```tsx
import { useArticlesByTag } from '@/lib/api';

const { data } = useArticlesByTag('TypeScript');
```

#### いいねの切り替え

```tsx
import { useToggleLike } from '@/lib/api';

const toggleLike = useToggleLike();

toggleLike.mutate(articleId);
```

### 3. ユーザープロフィール

#### プロフィール取得

```tsx
import { useUserProfile } from '@/lib/api';

const { data, isLoading } = useUserProfile(userId);
```

#### プロフィール更新

```tsx
import { useUpdateUserProfile } from '@/lib/api';

const updateProfile = useUpdateUserProfile();

updateProfile.mutate({
  name: '新しい名前',
  bio: '自己紹介',
  profileImage: 'https://example.com/image.jpg',
});
```

## 直接API関数を呼び出す

TanStack Queryのフックを使わずに、直接API関数を呼び出すこともできます：

```tsx
import { articlesApi, authApi, usersApi } from '@/lib/api';

// 認証
const authResponse = await authApi.signIn({
  usernameOrEmail: 'test@example.com',
  password: 'Password123!',
});

// 記事取得
const article = await articlesApi.getById(1);

// ユーザープロフィール取得
const profile = await usersApi.getProfile(1);
```

## キャッシュとリフェッチ

TanStack Queryは自動的にデータをキャッシュし、以下の場合にリフェッチします：

- Mutationが成功した後（関連するクエリが無効化されます）
- `staleTime`（1分）が経過した後
- ウィンドウがフォーカスされた時（無効化されています）

### 手動でキャッシュを無効化する

```tsx
import { useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/lib/api';

const queryClient = useQueryClient();

// 特定の記事のキャッシュを無効化
queryClient.invalidateQueries({ queryKey: queryKeys.articles.byId(1) });

// すべての記事のキャッシュを無効化
queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
```

## エラーハンドリング

```tsx
const createArticle = useCreateArticle({
  onError: (error) => {
    if (error.response?.status === 401) {
      // 認証エラー
      console.error('認証が必要です');
    } else if (error.response?.status === 400) {
      // バリデーションエラー
      console.error('入力値が不正です');
    } else {
      // その他のエラー
      console.error('エラーが発生しました:', error.message);
    }
  },
});
```

## 型定義

すべてのリクエスト・レスポンスの型は`src/lib/api/types.ts`で定義されています。

```tsx
import type {
  SignUpRequest,
  AuthResponse,
  CreateArticleRequest,
  GetArticleResponse,
} from '@/lib/api/types';
```

## 認証トークンの管理

認証トークンは自動的に管理されます：

1. サインイン/サインアップ成功時に`localStorage`に保存
2. すべてのAPIリクエストに自動的に追加
3. 401エラー時に自動的に削除

手動でトークンを管理する必要がある場合：

```tsx
// トークンを取得
const token = localStorage.getItem('authToken');

// トークンを設定
localStorage.setItem('authToken', 'your-token');

// トークンを削除
localStorage.removeItem('authToken');
```

## Container/Presentationalパターンでの使用

```tsx
// Container Component
'use client';

import { useArticle } from '@/lib/api';
import { ArticleView } from './ArticleView';

export function ArticleContainer({ articleId }: { articleId: number }) {
  const { data, isLoading, error } = useArticle(articleId);

  return (
    <ArticleView
      article={data}
      isLoading={isLoading}
      error={error}
    />
  );
}

// Presentational Component
export function ArticleView({ article, isLoading, error }) {
  if (isLoading) return <div>読み込み中...</div>;
  if (error) return <div>エラー: {error.message}</div>;
  if (!article) return null;

  return (
    <article>
      <h1>{article.title}</h1>
      <div>{article.body}</div>
    </article>
  );
}
```

## まとめ

- TanStack Queryを使用してAPIとの通信を管理
- すべてのAPI関数は型安全
- 自動的なキャッシュとリフェッチ
- 認証トークンは自動管理
- エラーハンドリングが簡単

詳細については、以下のファイルを参照してください：

- `src/lib/api/types.ts` - 型定義
- `src/lib/api/endpoints.ts` - API関数
- `src/lib/api/hooks.ts` - TanStack Query フック
- `src/lib/api/client.ts` - Axiosクライアント設定
