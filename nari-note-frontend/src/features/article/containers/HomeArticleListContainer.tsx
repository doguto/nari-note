'use client';

import { useGetArticles } from '@/lib/api';
import { HomeArticleCard } from '@/components/common/HomeArticleCard';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

/**
 * ホームページ用記事一覧コンテナ - Container Component
 * 
 * 新着記事一覧を取得して表示します。
 */
export function HomeArticleListContainer() {
  const { data, isLoading, error, refetch } = useGetArticles({ limit: 20, offset: 0 });

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

  if (!data?.articles || data.articles.length === 0) {
    return <p className="text-gray-500 text-center py-8">まだ記事がありません</p>;
  }

  return (
    <div className="space-y-4">
      {data.articles.map((article) => (
        <HomeArticleCard
          key={article.id}
          id={article.id ?? 0}
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
