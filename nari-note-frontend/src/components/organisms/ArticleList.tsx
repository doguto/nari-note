import { ReactNode } from 'react';
import { ArticleCard } from '@/components/molecules';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import type { ArticleThumbnailDto } from '@/lib/api';

interface ArticleListProps {
  articles?: ArticleThumbnailDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
  emptyMessage?: string;
  emptyIcon?: ReactNode;
  emptyVariant?: 'default' | 'panel';
  showAuthor?: boolean;
}

/**
 * ArticleList - Organism Component
 * 
 * 記事一覧を表示する汎用コンポーネント
 * ローディング、エラー、空状態のハンドリングを含む
 */
export function ArticleList({
  articles,
  isLoading,
  error,
  onRetry,
  emptyMessage = '記事がありません',
  emptyIcon,
  emptyVariant = 'default',
  showAuthor = true,
}: ArticleListProps) {
  if (isLoading) {
    return <LoadingSpinner text="記事を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={onRetry}
      />
    );
  }

  if (!articles || articles.length === 0) {
    if (emptyVariant === 'panel') {
      return <EmptyState variant="panel" title={emptyMessage} icon={emptyIcon} />;
    }
    return <p className="text-gray-500 text-center py-8">{emptyMessage}</p>;
  }

  // IDが存在しない記事をフィルタリング
  const articlesWithId = articles.filter((article) => article.id != null);

  if (articlesWithId.length === 0) {
    return <p className="text-gray-500 text-center py-8">有効な記事がありません</p>;
  }

  return (
    <div className="space-y-4">
      {articlesWithId.map((article) => (
        <ArticleCard
          key={article.id}
          id={article.id as string}
          title={article.title ?? ''}
          author={article.authorName ?? ''}
          authorId={article.authorId ?? ''}
          userIconImageUrl={article.userIconImageUrl}
          tags={article.tags ?? []}
          likeCount={article.likeCount ?? 0}
          date={article.updatedAt ? new Date(article.updatedAt).toLocaleDateString('ja-JP') : ''}
          showAuthor={showAuthor}
        />
      ))}
    </div>
  );
}
