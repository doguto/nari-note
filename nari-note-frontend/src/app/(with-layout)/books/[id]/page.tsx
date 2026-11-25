'use client';

import { Suspense } from 'react';
import { useParams } from 'next/navigation';
import { LoadingSpinner } from '@/components/ui';
import { AuthGuard } from '@/features/global/organisms';
import { BookResearchPage } from '@/features/book/pages';

export default function BookResearchRoute() {
  const params = useParams();
  const bookId = Number(params.id);

  return (
    <AuthGuard redirectPath={`/books/${bookId}`}>
      <Suspense fallback={<LoadingSpinner text="定跡を読み込み中..." />}>
        <BookResearchPage bookId={bookId} />
      </Suspense>
    </AuthGuard>
  );
}
