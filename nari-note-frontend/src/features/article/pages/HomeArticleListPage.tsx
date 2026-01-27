'use client';

import { useGetArticles } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { HomeArticleListTemplate } from '../templates/HomeArticleListTemplate';

/**
 * HomeArticleListPage - Page Component
 * 
 * ホーム画面の記事一覧のロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function HomeArticleListPage() {
  const { data, isLoading, error, refetch } = useGetArticles({ limit: 20, offset: 0 });

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

  if (!data?.articles || data.articles.length === 0) {
    return <HomeArticleListTemplate articles={[]} />;
  }

  // IDが存在しない記事をフィルタリング
  const articlesWithId = data.articles.filter((article) => article.id !== null && article.id !== undefined);

  return <HomeArticleListTemplate articles={articlesWithId} />;
}
