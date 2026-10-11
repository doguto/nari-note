
import type { Metadata } from 'next';
import { AuthGuard, VerifiedEmailGuard } from '@/features/global/organisms';
import { ArticleFormPage } from '@/features/article/pages';

export const metadata: Metadata = {
  robots: {
    index: false,
    follow: false,
  },
};

export default async function NewArticlePage({
  searchParams,
}: {
  searchParams: Promise<{ courseId?: string }>;
}) {
  const { courseId } = await searchParams;

  return (
    <AuthGuard redirectPath='/articles/new'>
      <VerifiedEmailGuard>
        <ArticleFormPage courseId={courseId} />
      </VerifiedEmailGuard>
    </AuthGuard>
  );
}
