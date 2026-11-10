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
    <div className="flex items-start gap-3 py-4 first:pt-0">
      <UserAvatar username={comment.userName || 'Unknown'} userIconImageUrl={comment.userIconImageUrl} size="sm" />
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-baseline gap-x-2">
          <span className="font-medium text-brand-text">{comment.userName}</span>
          <span className="text-xs text-gray-500">{formatDate(comment.createdAt)}</span>
        </div>
        <p className="mt-1 text-gray-800 whitespace-pre-wrap break-words">{comment.message}</p>
      </div>
    </div>
  );
}
