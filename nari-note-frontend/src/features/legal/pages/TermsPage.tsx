import { PageWithoutSidebar } from '@/features/global/organisms';
import { TermsTemplate } from '../templates/TermsTemplate';

/**
 * TermsPage - Page Component
 *
 * 利用規約ページ
 */
export function TermsPage() {
  return (
    <PageWithoutSidebar>
      <TermsTemplate />
    </PageWithoutSidebar>
  );
}
