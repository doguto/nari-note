'use client';

import { Suspense } from 'react';
import { useParams } from 'next/navigation';
import { LoadingSpinner } from '@/components/ui';
import { AuthGuard } from '@/features/global/organisms';
import { PlaybookResearchPage } from '@/features/playbook/pages';

export default function PlaybookResearchRoute() {
  const params = useParams();
  const playbookId = params.id as string;

  return (
    <AuthGuard redirectPath={`/playbooks/${playbookId}`}>
      <Suspense fallback={<LoadingSpinner text="定跡を読み込み中..." />}>
        <PlaybookResearchPage playbookId={playbookId} />
      </Suspense>
    </AuthGuard>
  );
}
