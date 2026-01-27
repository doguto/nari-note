'use client';

import { ArticleCard } from '@/components/molecules';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import type { ArticleDto } from '@/lib/api';

interface ArticleListProps {
  articles?: ArticleDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
  emptyMessage?: string;
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
          id={article.id as number}
          title={article.title ?? ''}
          author={article.authorName ?? ''}
          authorId={article.authorId ?? 0}
          stats={`いいね ${article.likeCount ?? 0}`}
          date={article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
          image="📝"
        />
      ))}
    </div>
  );
}
