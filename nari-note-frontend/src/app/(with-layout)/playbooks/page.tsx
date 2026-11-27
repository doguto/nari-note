import type { Metadata } from 'next';
import { AuthGuard } from '@/features/global/organisms';
import { MyPlaybooksPage } from '@/features/playbook/pages';

export const metadata: Metadata = {
  title: 'マイ定跡',
  robots: {
    index: false,
    follow: false,
  },
};

export default function PlaybooksPage() {
  return (
    <AuthGuard redirectPath="/playbooks">
      <MyPlaybooksPage />
    </AuthGuard>
  );
}
