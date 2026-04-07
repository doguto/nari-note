import { Comment } from '@/types/comment';
import { UserAvatar } from '@/components/ui';

interface CommentItemProps {
  comment: Comment;
}

/**
 * CommentItem - Molecule Component
 * 
 * 個別のコメント表示コンポーネント
 */
export function CommentItem({ comment }: CommentItemProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString('ja-JP', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="bg-gray-50 rounded-lg p-4 mb-3">
      <div className="flex items-start gap-3">
        <UserAvatar username={comment.userName || 'Unknown'} size="sm" />
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <span className="font-medium text-brand-text">{comment.userName}</span>
            <span className="text-xs text-gray-500">{formatDate(comment.createdAt)}</span>
          </div>
          <p className="text-gray-800 whitespace-pre-wrap">{comment.message}</p>
        </div>
      </div>
    </div>
  );
}
