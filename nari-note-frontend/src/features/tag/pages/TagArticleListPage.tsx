'use client';

import { useGetArticlesByTag } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { TagArticleListTemplate } from '../templates/TagArticleListTemplate';

interface TagArticleListPageProps {
  tag: string;
}

/**
 * TagArticleListPage - Page Component
 * 
 * タグ別記事一覧ページのビジネスロジックを担当するページコンポーネント
 * データフェッチング、状態管理、イベントハンドリングを行い、Templateにpropsを渡す
 */
export function TagArticleListPage({ tag }: TagArticleListPageProps) {
  const { data, isLoading, error, refetch } = useGetArticlesByTag({ tagName: tag });

  if (isLoading) {
    return <LoadingSpinner text="記事を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  const articles = data?.articles ?? [];

  return <TagArticleListTemplate tag={tag} articles={articles} />;
}
