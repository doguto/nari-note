'use client';

import { AuthGuard } from '@/features/global/organisms';
import { ProfileEditPage } from '@/features/user/pages';

/**
 * プロフィール編集ページ
 * 
 * ユーザーがプロフィール情報を編集できるページ
 */
export default function SettingsProfilePage() {
  return (
    <AuthGuard redirectPath="/settings/profile">
      <ProfileEditPage />
    </AuthGuard>
  );
}
