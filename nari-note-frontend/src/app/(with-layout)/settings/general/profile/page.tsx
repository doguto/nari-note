'use client';

import { AuthGuard } from '@/features/global/organisms';
import { ProfileEditPage } from '@/features/user/pages';

export default function SettingsProfilePage() {
  return (
    <AuthGuard redirectPath="/settings/general/profile">
      <ProfileEditPage />
    </AuthGuard>
  );
}
