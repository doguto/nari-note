import { useInfiniteQuery } from '@tanstack/react-query';
import { articlesApi, queryKeys } from '@/lib/api';
import { Comment } from '@/types/comment';

const PAGE_SIZE = 20;

/**
 * 記事のコメントを古い順に PAGE_SIZE 件ずつ読み込む
 *
 * キーを articles 配下にしているため、コメント投稿時の articles の invalidate で読み込み済みのページごと再取得される
 */
export function useArticleComments(articleId: string) {
  const query = useInfiniteQuery({
    queryKey: [...queryKeys.articles.getArticleComments, { id: articleId }],
    queryFn: ({ pageParam }) => articlesApi.getArticleComments({ id: articleId, limit: PAGE_SIZE, offset: pageParam }),
    initialPageParam: 0,
    getNextPageParam: (lastPage, allPages) => {
      const loadedCount = allPages.reduce((sum, page) => sum + page.comments.length, 0);
      return lastPage.comments.length > 0 && loadedCount < lastPage.totalCount ? loadedCount : undefined;
    },
  });

  const pages = query.data?.pages ?? [];
  const comments: Comment[] = pages.flatMap((page) => page.comments);
  const totalCount = pages.at(-1)?.totalCount ?? 0;

  return {
    comments,
    totalCount,
    hasMore: query.hasNextPage,
    isLoading: query.isLoading,
    isError: query.isError,
    isFetchingMore: query.isFetchingNextPage,
    loadMore: () => query.fetchNextPage(),
    retry: () => query.refetch(),
  };
}
