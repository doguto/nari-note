'use client';

import { useState } from 'react';
import { useGetArticleContent, useToggleLike } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import { PageWithSidebar } from '@/features/global/organisms';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Comment } from '@/types/comment';

interface ArticleDetailPageProps {
  articleId: string;
  justPublished?: boolean;
}

export function ArticleDetailPage({ articleId, justPublished = false }: ArticleDetailPageProps) {
  const { userId } = useAuth();
  const [isPublishedBannerDismissed, setIsPublishedBannerDismissed] = useState(false);
  const { ensureVerifiedEmail } = useRequireVerifiedEmail();
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
    if (!ensureVerifiedEmail()) return;
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
    <PageWithSidebar wide>
      <ArticleDetailTemplate
      isLoading={isLoading}
      error={error}
      article={article}
      comments={comments}
      isOwnArticle={isOwnArticle}
      isLikePending={isLikePending}
      showPublishedBanner={justPublished && isOwnArticle && !isPublishedBannerDismissed}
      onDismissPublishedBanner={() => setIsPublishedBannerDismissed(true)}
      onRetry={refetch}
      onLikeClick={handleLikeClick}
      onCommentSuccess={handleCommentSuccess}
      />
    </PageWithSidebar>
  );
}
