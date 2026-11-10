import { Comment } from '@/types/comment';
import { CommentItem } from './CommentItem';

interface CommentListProps {
  comments: Comment[];
}

/**
 * CommentList - Organism Component
 *
 * コメント一覧を表示するコンポーネント
 */
export function CommentList({ comments }: CommentListProps) {
  return (
    <div>
      <h2 className="text-lg font-bold text-brand-text mb-4">
        コメント
        {comments.length > 0 && <span className="ml-2 font-medium text-gray-500">{comments.length}</span>}
      </h2>
      {comments.length === 0 ? (
        <p className="text-sm text-gray-500">まだコメントがありません</p>
      ) : (
        <div className="divide-y divide-gray-100">
          {comments.map((comment) => (
            <CommentItem key={comment.id} comment={comment} />
          ))}
        </div>
      )}
    </div>
  );
}
