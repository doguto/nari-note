'use client';

import { useGetArticlesByTag } from '@/lib/api';
import { HomeArticleCard } from '@/components/common/HomeArticleCard';
import { EmptyState } from '@/components/common/EmptyState';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface TagArticleListPageProps {
  tag: string;
}

/**
 * TagArticleListPage - Organism Component
 * 
 * タグ別記事一覧ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function TagArticleListPage({ tag }: TagArticleListPageProps) {
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

  const articles = data?.articles ?? [];

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-3xl font-bold text-brand-text mb-2">
          #{tag}
        </h1>
        <p className="text-gray-600">
          {articles.length}件の記事
        </p>
      </div>

      {articles.length === 0 ? (
        <EmptyState
          icon="📝"
          title="記事がありません"
          description={`#${tag} に関連する記事はまだ投稿されていません。`}
        />
      ) : (
        <div className="space-y-4">
          {articles.map((article) => (
            <HomeArticleCard
              key={article.id}
              id={article.id!}
              title={article.title ?? ''}
              author={article.authorName ?? ''}
              authorId={article.authorId ?? 0}
              stats={`いいね ${article.likeCount ?? 0}`}
              date={article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
              image="📝"
            />
          ))}
        </div>
      )}
    </div>
  );
}
