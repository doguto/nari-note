# フロントエンド実装ガイド（AI エージェント向け）

このドキュメントは、AIエージェントがnari-note-frontendのコードを生成する際の具体的なガイドラインです。

## 目次

1. [コード生成の基本ルール](#コード生成の基本ルール)
2. [ディレクトリ配置ルール](#ディレクトリ配置ルール)
3. [命名規則](#命名規則)
4. [コンポーネント生成パターン](#コンポーネント生成パターン)
5. [よくあるパターン](#よくあるパターン)

## コード生成の基本ルール

### 1. Container/Presentationalパターンを必ず使用

データを扱うコンポーネントは、必ずContainerとPresentationalに分離してください。

```tsx
// ❌ 悪い例: 1つのコンポーネントにすべてを詰め込む
export function ArticleCard({ articleId }: { articleId: number }) {
  const { data } = useGetArticle({ id: articleId });
  return <div>{data?.title}</div>;
}

// ✅ 良い例: Container/Presentationalに分離
// Container
export function ArticleCardContainer({ articleId }: { articleId: number }) {
  const { data, isLoading, error } = useGetArticle({ id: articleId });
  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="エラー" />;
  if (!data) return null;
  return <ArticleCard article={data} />;
}

// Presentational
export function ArticleCard({ article }: { article: GetArticleResponse }) {
  return <div>{article.title}</div>;
}
```

### 2. 型定義を明確にする

すべてのpropsに型定義を追加してください。

```tsx
// ✅ 良い例
interface ArticleCardProps {
  article: GetArticleResponse;
  onLike?: () => void;
  showAuthor?: boolean;
}

export function ArticleCard({ article, onLike, showAuthor = true }: ArticleCardProps) {
  // ...
}
```

### 3. 共通コンポーネントを活用する

Loading、ErrorMessage、EmptyStateなどの共通コンポーネントを使用してください。

```tsx
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { EmptyState } from '@/components/common/EmptyState';
```

## ディレクトリ配置ルール

### 機能コンポーネント（Article、Auth、Userなど）

```
src/features/{feature}/
├── components/          # Presentational Components
│   └── {ComponentName}.tsx
├── containers/          # Container Components
│   └── {ComponentName}Container.tsx
├── hooks/              # カスタムフック（必要な場合）
│   └── use{HookName}.ts
└── types.ts            # 型定義（必要な場合）
```

**例:**
```
src/features/article/
├── components/
│   ├── ArticleCard.tsx
│   ├── ArticleList.tsx
│   └── ArticleDetail.tsx
├── containers/
│   ├── ArticleCardContainer.tsx
│   ├── ArticleListContainer.tsx
│   └── ArticleDetailContainer.tsx
└── hooks/
    └── useArticleForm.ts
```

### 共通コンポーネント

```
src/components/
├── ui/                 # 基本UIコンポーネント
│   ├── Button.tsx
│   └── Input.tsx
├── layout/             # レイアウトコンポーネント
│   ├── Header.tsx
│   └── Footer.tsx
└── common/             # その他共通コンポーネント
    ├── Loading.tsx
    ├── ErrorMessage.tsx
    └── EmptyState.tsx
```

### ページコンポーネント

```
src/app/
├── page.tsx                    # トップページ
├── articles/
│   ├── page.tsx               # 記事一覧
│   ├── [id]/
│   │   └── page.tsx          # 記事詳細
│   └── new/
│       └── page.tsx          # 記事作成
└── (auth)/
    ├── login/
    │   └── page.tsx
    └── signup/
        └── page.tsx
```

## 命名規則

### コンポーネントファイル

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| Presentational Component | `{ComponentName}.tsx` | `ArticleCard.tsx` |
| Container Component | `{ComponentName}Container.tsx` | `ArticleCardContainer.tsx` |
| Page Component | `page.tsx` | `page.tsx` |
| Layout Component | `layout.tsx` | `layout.tsx` |

### 非コンポーネントファイル

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| カスタムフック | `use{HookName}.ts` | `useArticleForm.ts` |
| ユーティリティ | `{utilName}.ts` | `format.ts` |
| 型定義 | `types.ts` | `types.ts` |

### 変数・関数

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| コンポーネント | PascalCase | `ArticleCard` |
| 関数 | camelCase | `handleSubmit` |
| 定数 | UPPER_SNAKE_CASE | `API_BASE_URL` |
| カスタムフック | camelCase (use始まり) | `useArticleForm` |

## コンポーネント生成パターン

### パターン1: データ取得を伴う表示コンポーネント

**要件:** 記事詳細を表示するコンポーネントを作成

**生成するファイル:**
1. `src/features/article/components/ArticleDetail.tsx` (Presentational)
2. `src/features/article/containers/ArticleDetailContainer.tsx` (Container)

**Presentational Component:**
```tsx
// src/features/article/components/ArticleDetail.tsx
import type { GetArticleResponse } from '@/lib/api/types';

interface ArticleDetailProps {
  article: GetArticleResponse;
}

export function ArticleDetail({ article }: ArticleDetailProps) {
  return (
    <article className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold text-[#2d3e1f] mb-4">
        {article.title}
      </h1>
      <div className="flex items-center gap-4 mb-6 text-gray-600">
        <span>著者: {article.authorName}</span>
        <span>いいね: {article.likeCount}</span>
      </div>
      <div className="prose max-w-none">
        {article.body}
      </div>
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 mt-6">
          {article.tags.map((tag, index) => (
            <span
              key={index}
              className="px-3 py-1 bg-[#f5f3e8] rounded-full text-sm"
            >
              #{tag}
            </span>
          ))}
        </div>
      )}
    </article>
  );
}
```

**Container Component:**
```tsx
// src/features/article/containers/ArticleDetailContainer.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetail } from '../components/ArticleDetail';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleDetailContainerProps {
  articleId: number;
}

export function ArticleDetailContainer({ articleId }: ArticleDetailContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!data) {
    return <ErrorMessage message="記事が見つかりません" />;
  }

  return <ArticleDetail article={data} />;
}
```

### パターン2: フォームコンポーネント

**要件:** 記事作成フォームを作成

**生成するファイル:**
1. `src/features/article/components/ArticleForm.tsx` (Presentational)
2. `src/features/article/containers/ArticleFormContainer.tsx` (Container)
3. `src/features/article/hooks/useArticleForm.ts` (カスタムフック)

**Presentational Component:**
```tsx
// src/features/article/components/ArticleForm.tsx
interface ArticleFormProps {
  title: string;
  body: string;
  tags: string[];
  onTitleChange: (value: string) => void;
  onBodyChange: (value: string) => void;
  onTagsChange: (tags: string[]) => void;
  onSubmit: () => void;
  isSubmitting: boolean;
}

export function ArticleForm({
  title,
  body,
  tags,
  onTitleChange,
  onBodyChange,
  onTagsChange,
  onSubmit,
  isSubmitting,
}: ArticleFormProps) {
  return (
    <form onSubmit={(e) => { e.preventDefault(); onSubmit(); }}>
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-2">
            タイトル
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => onTitleChange(e.target.value)}
            className="w-full px-4 py-2 border rounded"
            placeholder="記事のタイトル"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-2">
            本文
          </label>
          <textarea
            value={body}
            onChange={(e) => onBodyChange(e.target.value)}
            className="w-full px-4 py-2 border rounded"
            rows={10}
            placeholder="記事の内容"
          />
        </div>
        
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939] disabled:opacity-50"
        >
          {isSubmitting ? '投稿中...' : '投稿する'}
        </button>
      </div>
    </form>
  );
}
```

**Custom Hook:**
```tsx
// src/features/article/hooks/useArticleForm.ts
import { useState } from 'react';
import { useCreateArticle } from '@/lib/api';
import { useRouter } from 'next/navigation';

export function useArticleForm() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  
  const router = useRouter();
  
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      router.push(`/articles/${data.id}`);
    },
  });

  const handleSubmit = () => {
    createArticle.mutate({
      title,
      body,
      tags,
    });
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    handleSubmit,
    isSubmitting: createArticle.isPending,
  };
}
```

**Container Component:**
```tsx
// src/features/article/containers/ArticleFormContainer.tsx
'use client';

import { ArticleForm } from '../components/ArticleForm';
import { useArticleForm } from '../hooks/useArticleForm';

export function ArticleFormContainer() {
  const {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    handleSubmit,
    isSubmitting,
  } = useArticleForm();

  return (
    <ArticleForm
      title={title}
      body={body}
      tags={tags}
      onTitleChange={setTitle}
      onBodyChange={setBody}
      onTagsChange={setTags}
      onSubmit={handleSubmit}
      isSubmitting={isSubmitting}
    />
  );
}
```

### パターン3: 一覧表示コンポーネント

**要件:** 記事一覧を表示

**生成するファイル:**
1. `src/features/article/components/ArticleList.tsx` (Presentational)
2. `src/features/article/containers/ArticleListContainer.tsx` (Container)

**Presentational Component:**
```tsx
// src/features/article/components/ArticleList.tsx
import type { GetArticleResponse } from '@/lib/api/types';
import { ArticleCard } from './ArticleCard';

interface ArticleListProps {
  articles: GetArticleResponse[];
}

export function ArticleList({ articles }: ArticleListProps) {
  if (articles.length === 0) {
    return (
      <EmptyState
        icon="📝"
        title="記事がありません"
        description="まだ記事が投稿されていません。"
      />
    );
  }

  return (
    <div className="space-y-4">
      {articles.map((article) => (
        <ArticleCard key={article.id} article={article} />
      ))}
    </div>
  );
}
```

**Container Component:**
```tsx
// src/features/article/containers/ArticleListContainer.tsx
'use client';

import { useGetArticlesByAuthor } from '@/lib/api';
import { ArticleList } from '../components/ArticleList';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleListContainerProps {
  authorId: number;
}

export function ArticleListContainer({ authorId }: ArticleListContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticlesByAuthor({ authorId });

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  return <ArticleList articles={data?.articles ?? []} />;
}
```

## よくあるパターン

### パターンA: ページコンポーネントの作成

**動的ルートのページ:**
```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/containers/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <div className="container mx-auto">
      <ArticleDetailContainer articleId={articleId} />
    </div>
  );
}
```

### パターンB: 認証が必要なページ

```tsx
// src/app/articles/new/page.tsx
'use client';

import { ArticleFormContainer } from '@/features/article/containers/ArticleFormContainer';

export default function NewArticlePage() {
  // 認証チェックは後で実装
  return (
    <div className="container mx-auto max-w-4xl">
      <h1 className="text-3xl font-bold mb-6">新規記事作成</h1>
      <ArticleFormContainer />
    </div>
  );
}
```

### パターンC: エラーハンドリング

```tsx
const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

if (isLoading) {
  return <Loading />;
}

if (error) {
  return (
    <ErrorMessage 
      message="記事の取得に失敗しました" 
      onRetry={refetch}
    />
  );
}

if (!data) {
  return <ErrorMessage message="記事が見つかりません" />;
}

return <ArticleDetail article={data} />;
```

## チェックリスト

新しいコンポーネントを生成する際は、以下を確認してください：

- [ ] Container/Presentationalパターンに分離されているか
- [ ] 型定義が明確に記述されているか
- [ ] `'use client'`ディレクティブが必要な場所（Container）に記述されているか
- [ ] 共通コンポーネント（Loading、ErrorMessage）を使用しているか
- [ ] nari-noteのブランドカラーを使用しているか
- [ ] ファイルが適切なディレクトリに配置されているか
- [ ] 命名規則に従っているか
- [ ] エラーハンドリングが実装されているか

## 関連ドキュメント

- [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md)
- [API使用方法](/docs/frontend-api-usage.md)
