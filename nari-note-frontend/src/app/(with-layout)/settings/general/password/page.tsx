'use client';

import { AuthGuard } from '@/features/global/organisms';
import { PasswordEditPage } from '@/features/user/pages';

export default function SettingsPasswordPage() {
  return (
    <AuthGuard redirectPath="/settings/general/password">
      <PasswordEditPage />
    </AuthGuard>
  );
}
