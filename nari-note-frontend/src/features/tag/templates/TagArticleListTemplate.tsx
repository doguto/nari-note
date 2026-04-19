import { ArticleCard } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import type { GetArticlesByTagResponse } from '@/lib/api/types';

interface TagArticleListTemplateProps {
  tag: string;
  articles: GetArticlesByTagResponse['articles'];
}

/**
 * TagArticleListTemplate - Template Component
 * 
 * タグ別記事一覧ページのUI構成を担当するテンプレートコンポーネント
 * レイアウトとコンポーネント配置を定義
 */
export function TagArticleListTemplate({ tag, articles }: TagArticleListTemplateProps) {
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
            <ArticleCard
              key={article.id}
              id={article.id!}
              title={article.title ?? ''}
              author={article.authorName ?? ''}
              authorId={article.authorId ?? 0}
              authorProfileImage={article.authorProfileImage}
              tags={article.tags ?? []}
              likeCount={article.likeCount ?? 0}
              date={article.publishedAt ? new Date(article.publishedAt).toLocaleDateString('ja-JP') : article.updatedAt ? new Date(article.updatedAt).toLocaleDateString('ja-JP') : ''}
            />
          ))}
        </div>
      )}
    </div>
  );
}
