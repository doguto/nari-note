# フロントエンドクイックリファレンス

開発者とAIエージェントのための簡易リファレンスです。

## ディレクトリ配置

| 何を作る？ | どこに配置？ | ファイル名 |
|-----------|------------|-----------|
| 記事表示コンポーネント | `src/features/article/components/` | `ArticleDetail.tsx` |
| 記事表示Container | `src/features/article/containers/` | `ArticleDetailContainer.tsx` |
| 記事フォーム用フック | `src/features/article/hooks/` | `useArticleForm.ts` |
| ボタンコンポーネント | `src/components/ui/` | `Button.tsx` |
| ヘッダーコンポーネント | `src/components/layout/` | `Header.tsx` |
| ローディング表示 | `src/components/common/` | `Loading.tsx` |
| 記事詳細ページ | `src/app/articles/[id]/` | `page.tsx` |
| 日付フォーマット関数 | `src/lib/utils/` | `format.ts` |
| カスタムフック（共通） | `src/lib/hooks/` | `useDebounce.ts` |

## コンポーネント作成テンプレート

### Presentational Component

```tsx
import type { GetArticleResponse } from '@/lib/api/types';

interface ArticleCardProps {
  article: GetArticleResponse;
  onLike?: () => void;
}

export function ArticleCard({ article, onLike }: ArticleCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-2xl font-bold">{article.title}</h2>
      {onLike && (
        <button onClick={onLike}>いいね</button>
      )}
    </div>
  );
}
```

### Container Component

```tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleCard } from '../components/ArticleCard';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleCardContainerProps {
  articleId: number;
}

export function ArticleCardContainer({ articleId }: ArticleCardContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="エラー" onRetry={refetch} />;
  if (!data) return null;

  return <ArticleCard article={data} />;
}
```

### Page Component

```tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/containers/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailContainer articleId={articleId} />;
}
```

### Custom Hook

```tsx
import { useState } from 'react';
import { useCreateArticle } from '@/lib/api';

export function useArticleForm() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  
  const createArticle = useCreateArticle({
    onSuccess: () => {
      // 成功時の処理
    },
  });

  const handleSubmit = () => {
    createArticle.mutate({ title, body, tags: [] });
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    handleSubmit,
    isSubmitting: createArticle.isPending,
  };
}
```

## API使用

### データ取得（GET）

```tsx
import { useGetArticle } from '@/lib/api';

const { data, isLoading, error, refetch } = useGetArticle({ id: 1 });
```

### データ作成・更新（POST/PUT）

```tsx
import { useCreateArticle } from '@/lib/api';

const createArticle = useCreateArticle({
  onSuccess: (data) => {
    console.log('成功', data);
  },
  onError: (error) => {
    console.error('エラー', error);
  },
});

createArticle.mutate({
  title: 'タイトル',
  body: '本文',
  tags: ['TypeScript'],
});
```

## よく使うインポート

```tsx
// API
import { useGetArticle, useCreateArticle } from '@/lib/api';
import type { GetArticleResponse } from '@/lib/api/types';

// 共通コンポーネント
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { EmptyState } from '@/components/common/EmptyState';

// Next.js
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
```

## スタイリング（Tailwind CSS）

### nari-noteブランドカラー

```tsx
// プライマリ（緑）
className="bg-[#88b04b] text-white"

// セカンダリ（ダークグリーン）
className="bg-[#2d3e1f] text-white"

// 背景（ベージュ）
className="bg-[#f5f3e8]"

// ボーダー
className="border border-[#d4cdb3]"
```

### よく使うクラス

```tsx
// コンテナ
className="max-w-7xl mx-auto px-4"

// カード
className="bg-white rounded-lg shadow p-6"

// ボタン（プライマリ）
className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939]"

// 入力フィールド
className="w-full px-4 py-2 border rounded"

// グリッドレイアウト
className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
```

## チェックリスト

新しいコンポーネントを作成したら：

- [ ] Container/Presentationalに分離されているか
- [ ] 型定義が明確か
- [ ] `'use client'`がContainerに記述されているか
- [ ] Loading/ErrorMessageを使用しているか
- [ ] ブランドカラーを使用しているか
- [ ] 適切なディレクトリに配置されているか

## 詳細ドキュメント

- [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) - 完全なアーキテクチャ説明
- [フロントエンド実装ガイド](/docs/frontend-implementation-guide.md) - 詳細な実装パターン
- [API使用方法](/docs/frontend-api-usage.md) - API使用の詳細
