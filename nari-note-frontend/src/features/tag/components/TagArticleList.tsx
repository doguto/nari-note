import type { GetArticleResponse } from '@/lib/api/types';
import { ArticleCard } from '@/features/article/components/ArticleCard';
import { EmptyState } from '@/components/common/EmptyState';

interface TagArticleListProps {
  tag: string;
  articles: GetArticleResponse[];
}

/**
 * タグ記事一覧 - Presentational Component
 * 
 * 特定のタグに紐づく記事一覧を表示します。
 */
export function TagArticleList({ tag, articles }: TagArticleListProps) {
  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-3xl font-bold text-[#2d3e1f] mb-2">
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
            <ArticleCard key={article.id} article={article} />
          ))}
        </div>
      )}
    </div>
  );
}
