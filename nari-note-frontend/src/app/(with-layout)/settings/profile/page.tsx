'use client';

import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';
import { ProfileEditPage } from '@/features/user/pages';

/**
 * プロフィール編集ページ
 * 
 * ユーザーがプロフィール情報を編集できるページ
 */
export default function SettingsProfilePage() {
  const { isLoggedIn, isLoading } = useRequireAuth('/settings/profile');

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <LoadingSpinner text="読み込み中..." />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return <ProfileEditPage />;
}
