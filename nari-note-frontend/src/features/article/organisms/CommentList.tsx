'use client';

import { Comment } from '@/types/comment';
import { CommentItem } from '@/components/common/molecules/CommentItem';
import { EmptyState } from '@/components/common/EmptyState';

interface CommentListProps {
  comments: Comment[];
}

/**
 * CommentList - Organism Component
 * 
 * コメント一覧を表示するコンポーネント
 * Atomic Designパターンにおける Organism として、
 * コメントリストの表示を管理
 */
export function CommentList({ comments }: CommentListProps) {
  if (comments.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-lg p-8">
        <h2 className="text-xl font-bold text-brand-text mb-4">コメント</h2>
        <EmptyState title="まだコメントがありません" />
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <h2 className="text-xl font-bold text-brand-text mb-4">
        コメント ({comments.length})
      </h2>
      <div className="space-y-3">
        {comments.map((comment) => (
          <CommentItem key={comment.id} comment={comment} />
        ))}
      </div>
    </div>
  );
}
