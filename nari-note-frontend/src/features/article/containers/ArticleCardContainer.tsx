'use client';

import { useGetArticle, useToggleLike } from '@/lib/api';
import { ArticleCard } from '../components/ArticleCard';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleCardContainerProps {
  articleId: number;
}

/**
 * 記事カード Container Component
 * 
 * 記事データの取得といいね機能を管理します。
 */
export function ArticleCardContainer({ articleId }: ArticleCardContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });
  const toggleLike = useToggleLike();

  const handleLike = () => {
    toggleLike.mutate(
      { articleId },
      {
        onSuccess: () => {
          // 記事データを再取得してlikeCountを更新
          refetch();
        },
      }
    );
  };

  if (isLoading) {
    return <Loading size="sm" text="" />;
  }

  if (error) {
    // TODO: バックエンドから自動生成されるエラーハンドリングを使用する予定
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!data) {
    return null;
  }

  return <ArticleCard article={data} onLike={handleLike} />;
}
