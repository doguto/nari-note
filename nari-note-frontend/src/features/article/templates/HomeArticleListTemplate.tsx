import { ArticleCard } from '@/components/molecules';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { ArticleDto } from '@/lib/api/types';

interface HomeArticleListTemplateProps {
  articles: ArticleDto[];
  isLoading?: boolean;
  error?: Error | null;
  onRetry?: () => void;
}

/**
 * HomeArticleListTemplate - Template Component
 * 
 * ホーム画面の記事一覧のUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function HomeArticleListTemplate({ articles, isLoading, error, onRetry }: HomeArticleListTemplateProps) {
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
