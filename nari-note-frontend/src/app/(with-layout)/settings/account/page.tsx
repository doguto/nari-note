'use client';

import { AuthGuard } from '@/features/global/organisms';
import { AccountSettingsPage } from '@/features/settings/pages';

export default function SettingsAccountPage() {
  return (
    <AuthGuard redirectPath="/settings/account">
      <AccountSettingsPage />
    </AuthGuard>
  );
}
