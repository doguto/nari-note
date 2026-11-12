'use client';

import { useState } from 'react';
import { useGetArticleContent, useToggleLike } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import { PageWithSidebar } from '@/features/global/organisms';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { useArticleComments } from '../hooks/useArticleComments';

interface ArticleDetailPageProps {
  articleId: string;
  justPublished?: boolean;
}

export function ArticleDetailPage({ articleId, justPublished = false }: ArticleDetailPageProps) {
  const { userId } = useAuth();
  const [isPublishedBannerDismissed, setIsPublishedBannerDismissed] = useState(false);
  const { ensureVerifiedEmail } = useRequireVerifiedEmail();
  const { data: article, isLoading, error, refetch } = useGetArticleContent({ id: articleId });
  const articleComments = useArticleComments(articleId);
  const { mutate: toggleLike, isPending: isLikePending } = useToggleLike({
    onSuccess: () => {
      refetch();
    },
  });

  const handleLikeClick = () => {
    if (isLikePending) return;
    if (!ensureVerifiedEmail()) return;
    toggleLike({ articleId: articleId });
  };

  // 自分の記事かどうかを判定
  const isOwnArticle = userId === article?.article?.authorId;

  return (
    <PageWithSidebar wide>
      <ArticleDetailTemplate
      isLoading={isLoading}
      error={error}
      article={article}
      comments={{
        comments: articleComments.comments,
        totalCount: articleComments.totalCount,
        hasMore: articleComments.hasMore,
        isLoading: articleComments.isLoading,
        isError: articleComments.isError,
        isFetchingMore: articleComments.isFetchingMore,
        onLoadMore: articleComments.loadMore,
        onRetry: articleComments.retry,
      }}
      isOwnArticle={isOwnArticle}
      isLikePending={isLikePending}
      showPublishedBanner={justPublished && isOwnArticle && !isPublishedBannerDismissed}
      onDismissPublishedBanner={() => setIsPublishedBannerDismissed(true)}
      onRetry={refetch}
      onLikeClick={handleLikeClick}
      />
    </PageWithSidebar>
  );
}
