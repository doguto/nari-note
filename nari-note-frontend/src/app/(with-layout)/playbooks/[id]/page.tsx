'use client';

import { useParams } from 'next/navigation';
import { AuthGuard } from '@/features/global/organisms';
import { PlaybookResearchPage } from '@/features/playbook/pages';

export default function PlaybookResearchRoute() {
  const params = useParams();
  const playbookId = params.id as string;

  return (
    <AuthGuard redirectPath={`/playbooks/${playbookId}`}>
      <PlaybookResearchPage playbookId={playbookId} />
    </AuthGuard>
  );
}
