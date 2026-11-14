import { Users } from 'lucide-react';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { UserListItem } from '@/components/molecules';
import type { FollowerUserDto } from '@/lib/api/types';

interface UserFollowersListProps {
  users?: FollowerUserDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
  loadingText: string;
  errorMessage: string;
  emptyMessage: string;
}

/**
 * UserFollowersList - Organism Component
 *
 * フォロワー/フォロー中のユーザー一覧
 * ローディング、エラー、空状態、一覧表示を管理
 */
export function UserFollowersList({
  users,
  isLoading,
  error,
  onRetry,
  loadingText,
  errorMessage,
  emptyMessage,
}: UserFollowersListProps) {
  return (
    <div>
      {isLoading && (
        <div className="py-8">
          <LoadingSpinner text={loadingText} />
        </div>
      )}

      {error && (
        <div className="py-4">
          <ErrorMessage
            message={errorMessage}
            onRetry={onRetry}
          />
        </div>
      )}

      {!isLoading && !error && users && users.length === 0 && (
        <EmptyState variant="panel" title={emptyMessage} icon={<Users />} />
      )}

      {!isLoading && !error && users && users.length > 0 && (
        <div className="space-y-1 rounded-xl border border-gray-200 bg-white p-2 sm:p-4">
          {users.map((user) => (
            user.id && user.username ? (
              <UserListItem
                key={user.id}
                userId={user.id}
                username={user.username}
                userIconImageUrl={user.userIconImageUrl}
              />
            ) : null
          ))}
        </div>
      )}
    </div>
  );
}
