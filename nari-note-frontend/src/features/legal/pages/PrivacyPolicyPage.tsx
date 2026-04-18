import { PageWithoutSidebar } from '@/features/global/organisms';
import { PrivacyPolicyTemplate } from '../templates/PrivacyPolicyTemplate';

/**
 * PrivacyPolicyPage - Page Component
 *
 * プライバシーポリシーページ
 */
export function PrivacyPolicyPage() {
  return (
    <PageWithoutSidebar>
      <PrivacyPolicyTemplate />
    </PageWithoutSidebar>
  );
}
