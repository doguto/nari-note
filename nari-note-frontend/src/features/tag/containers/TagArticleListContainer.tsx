'use client';

import { useGetArticlesByTag } from '@/lib/api';
import { TagArticleList } from '../components/TagArticleList';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface TagArticleListContainerProps {
  tag: string;
}

/**
 * タグ記事一覧コンテナ - Container Component
 * 
 * タグに紐づく記事一覧を取得してTagArticleListに渡します。
 */
export function TagArticleListContainer({ tag }: TagArticleListContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticlesByTag({ tagName: tag });

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

  return <TagArticleList tag={tag} articles={data?.articles ?? []} />;
}
