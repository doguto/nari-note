import { ChevronDown } from 'lucide-react';
import { Comment } from '@/types/comment';
import { Button } from '@/components/ui/button';
import { CommentItem } from './CommentItem';

export interface CommentListProps {
  comments: Comment[];
  totalCount: number;
  hasMore: boolean;
  isLoading: boolean;
  isError: boolean;
  isFetchingMore: boolean;
  onLoadMore: () => void;
  onRetry: () => void;
}

/**
 * CommentList - Organism Component
 *
 * コメント一覧を古い順に表示し、続きを「さらに表示」で追加読み込みする
 */
export function CommentList({
  comments,
  totalCount,
  hasMore,
  isLoading,
  isError,
  isFetchingMore,
  onLoadMore,
  onRetry,
}: CommentListProps) {
  const renderBody = () => {
    if (isLoading) {
      return <p className="text-sm text-gray-500">コメントを読み込み中...</p>;
    }
    // 読み込み済みのコメントがあれば、続きの取得に失敗しても表示は残す
    if (isError && comments.length === 0) {
      return (
        <div className="flex items-center gap-3">
          <p className="text-sm text-gray-500">コメントの取得に失敗しました</p>
          <Button type="button" variant="outline" size="sm" onClick={onRetry}>
            再読み込み
          </Button>
        </div>
      );
    }
    if (comments.length === 0) {
      return <p className="text-sm text-gray-500">まだコメントがありません</p>;
    }
    return (
      <div className="divide-y divide-gray-100">
        {comments.map((comment) => (
          <CommentItem key={comment.id} comment={comment} />
        ))}
      </div>
    );
  };

  return (
    <div>
      <h2 className="text-lg font-bold text-brand-text mb-4">
        コメント
        {totalCount > 0 && <span className="ml-2 font-medium text-gray-500">{totalCount}</span>}
      </h2>
      {renderBody()}
      {hasMore && comments.length > 0 && (
        <div className="mt-4 flex flex-col items-center gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={onLoadMore}
            disabled={isFetchingMore}
            className="h-11 px-6 border-brand-border text-brand-text hover:border-brand-primary hover:bg-brand-bg-light"
          >
            {isFetchingMore ? '読み込み中...' : `コメントをさらに表示（残り ${totalCount - comments.length} 件）`}
            {!isFetchingMore && <ChevronDown aria-hidden="true" />}
          </Button>
          <p className="text-[13px] text-brand-secondary-text">
            {comments.length} / {totalCount} 件を表示中
          </p>
        </div>
      )}
    </div>
  );
}
