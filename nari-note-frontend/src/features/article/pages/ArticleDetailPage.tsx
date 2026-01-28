'use client';

import { useGetArticle, useToggleLike } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { useAuth } from '@/lib/providers/AuthProvider';
import { PageWithSidebar } from '@/features/global/organisms';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Comment } from '@/types/comment';

interface ArticleDetailPageProps {
  articleId: number;
}

/**
 * ArticleDetailPage - Page Component
 * 
 * 記事詳細ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
  const { userId } = useAuth();
  const { data: article, isLoading, error, refetch } = useGetArticle({ id: articleId });
  const { mutate: toggleLike, isPending: isLikePending } = useToggleLike({
    onSuccess: () => {
      refetch();
    },
  });

  const handleCommentSuccess = () => {
    refetch();
  };

  const handleLikeClick = () => {
    if (isLikePending) return;
    toggleLike({ articleId: articleId });
  };

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

  if (!article) {
    return <ErrorMessage message="記事が見つかりません" />;
  }

  // APIから取得したコメントをComment型に変換
  const comments: Comment[] = (article.comments || []).map(c => ({
    id: c.id || 0,
    userId: c.userId || 0,
    userName: c.userName || '',
    message: c.message || '',
    createdAt: c.createdAt || '',
  }));

  // 自分の記事かどうかを判定
  const isOwnArticle = userId === article.authorId;

  return (
    <PageWithSidebar>
      <ArticleDetailTemplate
        article={article}
        comments={comments}
        isOwnArticle={isOwnArticle}
        isLikePending={isLikePending}
        onLikeClick={handleLikeClick}
        onCommentSuccess={handleCommentSuccess}
      />
    </PageWithSidebar>
  );
}
