'use client';

import { AuthGuard } from '@/features/global/organisms';
import { WithdrawPage } from '@/features/settings/pages';

export default function SettingsWithdrawPage() {
  return (
    <AuthGuard redirectPath="/settings/general/withdraw">
      <WithdrawPage />
    </AuthGuard>
  );
}
