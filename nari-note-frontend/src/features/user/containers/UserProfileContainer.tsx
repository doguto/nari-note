'use client';

import { useGetUserProfile } from '@/lib/api';
import { UserProfile } from '../components/UserProfile';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface UserProfileContainerProps {
  userId: number;
}

/**
 * ユーザープロフィールコンテナ - Container Component
 * 
 * ユーザー情報を取得してUserProfileに渡します。
 */
export function UserProfileContainer({ userId }: UserProfileContainerProps) {
  const { data, isLoading, error, refetch } = useGetUserProfile({ id: userId });

  if (isLoading) {
    return <Loading text="ユーザー情報を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="ユーザー情報の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!data) {
    return <ErrorMessage message="ユーザーが見つかりません" />;
  }

  return <UserProfile user={data} />;
}
