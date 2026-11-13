import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FollowButton, ProfileStat, UserAvatar, XLogo } from '@/components/ui';
import type { GetUserProfileResponse } from '@/lib/api/types';

interface ProfileCardProps {
  user: GetUserProfileResponse;
  isOwnProfile: boolean;
  isFollowPending: boolean;
  onTabChange: (tab: string) => void;
  onFollowClick: () => void;
}


export function ProfileCard({
  user,
  isOwnProfile,
  isFollowPending,
  onTabChange,
  onFollowClick,
}: ProfileCardProps) {
  const username = user.username || 'Unknown User';
  const joinedAt = new Date(user.createdAt).toLocaleDateString('ja-JP', { year: 'numeric', month: 'long', timeZone: 'Asia/Tokyo' });

  return (
    <div className="px-4 pt-6 pb-5 sm:px-8 sm:pt-8 sm:pb-7">
      <div className="flex items-start justify-between gap-4">
        <UserAvatar username={username} userIconImageUrl={user.userIconImageUrl} size="xl" />
        {isOwnProfile ? (
          <Button variant="outline" asChild>
            <Link href="/settings/general/profile">プロフィール編集</Link>
          </Button>
        ) : (
          <FollowButton
            isFollowing={user.isFollowing || false}
            onClick={onFollowClick}
            disabled={isFollowPending}
          />
        )}
      </div>

      <h1 className="mt-4 text-[22px] font-bold text-brand-text sm:text-[28px]">{username}</h1>

      <div className="mt-1.5 flex flex-wrap gap-x-4 gap-y-1 text-[13px] text-gray-500 sm:text-sm">
        {user.xAccountName && (
          <a
            href={`https://x.com/${encodeURIComponent(user.xAccountName)}`}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-1.5 text-gray-600 hover:text-brand-text hover:underline"
          >
            <XLogo className="size-3.5" />
            <span>@{user.xAccountName}</span>
          </a>
        )}
        <span>{joinedAt}から利用</span>
      </div>

      {user.bio && (
        <p className="mt-3 max-w-2xl text-sm leading-7 text-gray-700 sm:text-[15px]">{user.bio}</p>
      )}

      <div className="mt-4 grid grid-cols-4 divide-x divide-brand-bg-gradient-to rounded-lg border border-brand-bg-gradient-to sm:flex sm:flex-wrap sm:gap-7 sm:divide-x-0 sm:border-0">
        <ProfileStat label="記事" count={user.articleCount ?? 0} onClick={() => onTabChange('articles')} />
        <ProfileStat label="講座" count={user.courseCount ?? 0} onClick={() => onTabChange('courses')} />
        <ProfileStat label="フォロワー" count={user.followerCount ?? 0} onClick={() => onTabChange('followers')} />
        <ProfileStat label="フォロー中" count={user.followingCount ?? 0} onClick={() => onTabChange('followings')} />
      </div>
    </div>
  );
}
