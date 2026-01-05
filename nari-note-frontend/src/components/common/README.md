# Common Components

アプリケーション全体で使用される共通のユーティリティコンポーネントを配置するディレクトリです。

## 概要

このディレクトリには、ローディング表示、エラーメッセージ、空状態表示など、
アプリケーション全体で共通して使用されるコンポーネントを配置します。

## コンポーネント例

### Loading.tsx
ローディング表示コンポーネント

```tsx
interface LoadingProps {
  size?: 'sm' | 'md' | 'lg';
  text?: string;
}

export function Loading({ size = 'md', text = '読み込み中...' }: LoadingProps) {
  const sizeClasses = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };

  return (
    <div className="flex flex-col items-center justify-center py-8">
      <div className={`animate-spin rounded-full border-b-2 border-[#88b04b] ${sizeClasses[size]}`} />
      {text && <p className="mt-2 text-gray-600">{text}</p>}
    </div>
  );
}
```

### ErrorMessage.tsx
エラーメッセージ表示コンポーネント

```tsx
interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
      <p className="text-red-700 font-medium mb-2">エラーが発生しました</p>
      <p className="text-red-600 text-sm mb-4">{message}</p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
        >
          再試行
        </button>
      )}
    </div>
  );
}
```

### EmptyState.tsx
空状態表示コンポーネント

```tsx
interface EmptyStateProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12">
      {icon && <div className="text-6xl mb-4">{icon}</div>}
      <h3 className="text-xl font-bold text-gray-800 mb-2">{title}</h3>
      {description && <p className="text-gray-600 mb-4">{description}</p>}
      {action && (
        <button
          onClick={action.onClick}
          className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939]"
        >
          {action.label}
        </button>
      )}
    </div>
  );
}
```

## 使用例

### Containerコンポーネントで使用

```tsx
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { EmptyState } from '@/components/common/EmptyState';
import { useGetArticle } from '@/lib/api';

export function ArticleDetailContainer({ articleId }: { articleId: number }) {
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
    return (
      <EmptyState
        icon="📝"
        title="記事が見つかりません"
        description="指定された記事は存在しないか、削除された可能性があります。"
      />
    );
  }

  return <ArticleDetail article={data} />;
}
```

## 作成ガイドライン

1. **アプリケーション全体で統一されたデザイン**を提供
2. **nari-noteのブランドカラー**を使用
3. **適切なデフォルト値**を設定
4. **柔軟なカスタマイズ**を可能にする

## その他のコンポーネント例

- **SuccessMessage.tsx** - 成功メッセージ
- **Pagination.tsx** - ページネーション
- **Breadcrumb.tsx** - パンくずリスト
- **Toast.tsx** - トースト通知
- **Skeleton.tsx** - スケルトンローディング
