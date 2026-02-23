'use client';

import { useGetArticles } from '@/lib/api';
import { MainContentSection } from '@/features/global/organisms';
import { HomeArticleListTemplate } from '../templates/HomeArticleListTemplate';

/**
 * HomeArticleListPage - Page Component
 * 
 * ホーム画面の記事一覧のロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function HomeArticleListPage() {
  const { data, isLoading, error, refetch } = useGetArticles({ limit: 20, offset: 0 });

  // IDが存在しない記事をフィルタリング
  const articlesWithId = data?.articles?.filter((article) => article.id !== null && article.id !== undefined) || [];

  return (
    <MainContentSection title="新着記事">
      <HomeArticleListTemplate
        articles={articlesWithId}
        isLoading={isLoading}
        error={error}
        onRetry={refetch}
      />
    </MainContentSection>
  );
}
