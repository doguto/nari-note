'use client';

import { PasswordEditPage } from './PasswordEditPage';
import { WithdrawPage } from './WithdrawPage';


export function AccountSettingsPage() {
  return (
    <div className="space-y-8">
      <PasswordEditPage />
      <WithdrawPage />
    </div>
  );
}
