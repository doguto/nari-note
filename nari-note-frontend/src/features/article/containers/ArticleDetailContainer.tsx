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
    return <Loading text="記事を読み込み中..." />;
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
