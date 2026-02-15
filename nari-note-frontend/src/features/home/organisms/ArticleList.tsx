import { ArticleCard } from '@/components/molecules';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { ArticleDto } from '@/lib/api/types';

interface ArticleListProps {
  articles: ArticleDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
}

/**
 * ArticleList - Organism Component
 * 
 * 記事一覧を表示するOrganismコンポーネント
 * ローディング、エラー、空状態のハンドリングを含む
 */
export function ArticleList({ articles, isLoading, error, onRetry }: ArticleListProps) {
  if (isLoading) {
    return <LoadingSpinner text="記事を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={onRetry}
      />
    );
  }

  if (articles.length === 0) {
    return <EmptyState title="まだ記事がありません" />;
  }

  return (
    <div className="space-y-4">
      {articles.map((article) => (
        <ArticleCard
          key={article.id}
          id={article.id!}
          title={article.title ?? ''}
          author={article.authorName ?? ''}
          authorId={article.authorId ?? 0}
          tags={article.tags ?? []}
          likeCount={article.likeCount ?? 0}
          date={article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
        />
      ))}
    </div>
  );
}
