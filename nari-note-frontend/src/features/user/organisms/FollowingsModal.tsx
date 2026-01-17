'use client';

import { useGetFollowings } from '@/lib/api';
import { UserListItem } from '@/components/common/molecules';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

interface FollowingsModalProps {
  userId: number;
  isOpen: boolean;
  onClose: () => void;
}

/**
 * FollowingsModal - Organism Component
 * 
 * フォロー中一覧モーダル
 * ユーザーがフォローしているユーザーを一覧表示
 */
export function FollowingsModal({ userId, isOpen, onClose }: FollowingsModalProps) {
  const { data, isLoading, error, refetch } = useGetFollowings(
    { id: userId },
    { enabled: isOpen } // モーダルが開いているときのみデータ取得
  );

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-md max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>フォロー中</DialogTitle>
        </DialogHeader>

        <div className="flex-1 overflow-y-auto">
          {isLoading && (
            <div className="py-8">
              <Loading text="フォロー中のユーザーを読み込み中..." />
            </div>
          )}

          {error && (
            <div className="py-4">
              <ErrorMessage 
                message="フォロー中のユーザーの取得に失敗しました" 
                onRetry={refetch}
              />
            </div>
          )}

          {!isLoading && !error && data?.followings && data.followings.length === 0 && (
            <div className="text-center py-8 text-gray-500">
              フォロー中のユーザーがいません
            </div>
          )}

          {!isLoading && !error && data?.followings && data.followings.length > 0 && (
            <div className="space-y-2">
              {data.followings.map((following) => (
                following.id && following.username ? (
                  <UserListItem
                    key={following.id}
                    userId={following.id}
                    username={following.username}
                    profileImage={following.profileImage}
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
