'use client';

import { useGetFollowers } from '@/lib/api';
import { UserListItem } from '@/components/molecules';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

interface FollowersModalProps {
  userId: number;
  isOpen: boolean;
  onClose: () => void;
}

/**
 * FollowersModal - Organism Component
 * 
 * フォロワー一覧モーダル
 * ユーザーのフォロワーを一覧表示
 */
export function FollowersModal({ userId, isOpen, onClose }: FollowersModalProps) {
  const { data, isLoading, error, refetch } = useGetFollowers(
    { userId: userId },
    { enabled: isOpen } // モーダルが開いているときのみデータ取得
  );

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-md max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>フォロワー</DialogTitle>
        </DialogHeader>

        <div className="flex-1 overflow-y-auto">
          {isLoading && (
            <div className="py-8">
              <LoadingSpinner text="フォロワーを読み込み中..." />
            </div>
          )}

          {error && (
            <div className="py-4">
              <ErrorMessage 
                message="フォロワーの取得に失敗しました" 
                onRetry={refetch}
              />
            </div>
          )}

          {!isLoading && !error && data?.followers && data.followers.length === 0 && (
            <div className="text-center py-8 text-gray-500">
              フォロワーがいません
            </div>
          )}

          {!isLoading && !error && data?.followers && data.followers.length > 0 && (
            <div className="space-y-2">
              {data.followers.map((follower) => (
                follower.id && follower.username ? (
                  <UserListItem
                    key={follower.id}
                    userId={follower.id}
                    username={follower.username}
                    profileImage={follower.profileImage}
                  />
                ) : null
              ))}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
