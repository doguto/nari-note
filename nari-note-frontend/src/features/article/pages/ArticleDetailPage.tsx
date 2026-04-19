'use client';

import { useGetArticleContent, useToggleLike } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { PageWithSidebar } from '@/features/global/organisms';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Comment } from '@/types/comment';

interface ArticleDetailPageProps {
  articleId: string;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
  const { userId } = useAuth();
  const { data: article, isLoading, error, refetch } = useGetArticleContent({ id: articleId });
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

  // APIから取得したコメントをComment型に変換
  const comments: Comment[] = (article?.comments || []).map(c => ({
    id: c.id || 0,
    userId: c.userId || '',
    userName: c.userName || '',
    userIconImageUrl: c.userIconImageUrl,
    message: c.message || '',
    createdAt: c.createdAt || '',
  }));

  // 自分の記事かどうかを判定
  const isOwnArticle = userId === article?.article?.authorId;

  return (
    <PageWithSidebar>
      <ArticleDetailTemplate
      isLoading={isLoading}
      error={error}
      article={article}
      comments={comments}
      isOwnArticle={isOwnArticle}
      isLikePending={isLikePending}
      onRetry={refetch}
      onLikeClick={handleLikeClick}
      onCommentSuccess={handleCommentSuccess}
      />
    </PageWithSidebar>
  );
}
