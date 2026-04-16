import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FollowButton, FollowStats, UserAvatar } from '@/components/ui';
import type { GetUserProfileResponse } from '@/lib/api/types';

interface ProfileCardProps {
  user: GetUserProfileResponse;
  isOwnProfile: boolean;
  isFollowPending: boolean;
  onArticlesClick: () => void;
  onTabChange: (tab: string) => void;
  onFollowClick: () => void;
}

/**
 * ProfileCard - Organism Component
 *
 * ユーザープロフィールカード
 * アバター、ユーザー名、bio、フォロー統計、フォローボタンを表示
 */
export function ProfileCard({
  user,
  isOwnProfile,
  isFollowPending,
  onArticlesClick,
  onTabChange,
  onFollowClick,
}: ProfileCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-4 sm:p-6">
      <div className="flex items-start gap-4 sm:gap-6">
        <UserAvatar
          username={user.username || 'Unknown User'}
          size="xl"
        />

        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2 mb-1 sm:mb-2">
            <h1 className="text-lg sm:text-3xl font-bold text-brand-text">
              {user.username || 'Unknown User'}
            </h1>
            <div className="flex-shrink-0">
              {isOwnProfile ? (
                <Link href="/settings/profile">
                  <Button variant="outline" size="sm">プロフィール編集</Button>
                </Link>
              ) : (
                <FollowButton
                  isFollowing={user.isFollowing || false}
                  onClick={onFollowClick}
                  disabled={isFollowPending}
                />
              )}
            </div>
          </div>

          <p className="text-gray-600 text-sm mb-3 sm:mb-4">
            @{user.username || 'unknown'}
          </p>

          {user.bio && (
            <p className="text-gray-700 text-sm sm:text-base mb-3 sm:mb-4">
              {user.bio}
            </p>
          )}

          <div className="flex flex-wrap gap-4 sm:gap-6 text-sm text-gray-600">
            <button
              onClick={onArticlesClick}
              className="hover:opacity-70 transition-opacity cursor-pointer"
            >
              <span className="font-bold text-brand-text">{user.articleCount ?? 0}</span>
              <span className="ml-1">記事</span>
            </button>
            <FollowStats
              label="フォロワー"
              count={user.followerCount || 0}
              onClick={() => onTabChange('followers')}
            />
            <FollowStats
              label="フォロー中"
              count={user.followingCount || 0}
              onClick={() => onTabChange('followings')}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
